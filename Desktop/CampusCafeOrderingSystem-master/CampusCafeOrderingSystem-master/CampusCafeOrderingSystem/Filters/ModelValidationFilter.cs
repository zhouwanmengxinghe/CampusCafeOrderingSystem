using CampusCafeOrderingSystem.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CampusCafeOrderingSystem.Filters
{
    /// <summary>
    /// Model validation filter
    /// </summary>
    public class ModelValidationFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors)
                    .Select(x => x.ErrorMessage)
                    .ToList();

                var response = ApiResponse<object>.ErrorResult("Data validation failed", errors);
                
                context.Result = new BadRequestObjectResult(response);
            }
        }
    }

    /// <summary>
    /// API response formatting filter
    /// </summary>
    public class ApiResponseFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            // Skip views, files, redirects and other non-JSON responses
            if (context.Result is ViewResult ||
                context.Result is FileResult ||
                context.Result is RedirectResult ||
                context.Result is RedirectToActionResult ||
                context.Result is RedirectToRouteResult ||
                context.Result is LocalRedirectResult)
            {
                return;
            }

            // Handle results containing object payload
            if (context.Result is ObjectResult objectResult)
            {
                var statusCode = objectResult.StatusCode ?? 200;
                var value = objectResult.Value;

                // Already ApiResponse<T>, no need to wrap again
                if (value != null)
                {
                    var valueType = value.GetType();
                    var isAlreadyApiResponse = valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(ApiResponse<>);
                    if (isAlreadyApiResponse)
                    {
                        return;
                    }
                }

                if (statusCode >= 200 && statusCode < 300)
                {
                    objectResult.Value = ApiResponse<object>.SuccessResult(value);
                    if (objectResult.StatusCode == null) objectResult.StatusCode = 200;
                }
                else
                {
                    string message = "Operation failed";
                    var errors = new List<string>();

                    if (value is ProblemDetails pd)
                    {
                        message = pd.Title ?? message;
                        if (!string.IsNullOrWhiteSpace(pd.Detail)) errors.Add(pd.Detail);
                    }
                    else if (value is ValidationProblemDetails vpd)
                    {
                        message = vpd.Title ?? "Data validation failed";
                        foreach (var e in vpd.Errors.SelectMany(kv => kv.Value)) errors.Add(e);
                    }
                    else if (value != null)
                    {
                        // Try to extract message/error(s) from anonymous object
                        var type = value.GetType();
                        var msgProp = type.GetProperty("message") ?? type.GetProperty("Message");
                        if (msgProp?.GetValue(value) is string m && !string.IsNullOrWhiteSpace(m))
                        {
                            message = m;
                        }
                        var errorsProp = type.GetProperty("errors") ?? type.GetProperty("Errors");
                        if (errorsProp?.GetValue(value) is IEnumerable<string> errList)
                        {
                            errors.AddRange(errList);
                        }
                        else
                        {
                            var errorProp = type.GetProperty("error") ?? type.GetProperty("Error");
                            if (errorProp?.GetValue(value) is string e && !string.IsNullOrWhiteSpace(e))
                            {
                                errors.Add(e);
                            }
                        }
                    }

                    objectResult.Value = ApiResponse<object>.ErrorResult(message, errors);
                }
                return;
            }

            // Handle JsonResult
            if (context.Result is JsonResult jsonResult)
            {
                var statusCode = jsonResult.StatusCode ?? 200;
                if (statusCode >= 200 && statusCode < 300)
                {
                    context.Result = new ObjectResult(ApiResponse<object>.SuccessResult(jsonResult.Value))
                    {
                        StatusCode = 200
                    };
                }
                else
                {
                    context.Result = new ObjectResult(ApiResponse<object>.ErrorResult("Operation failed"))
                    {
                        StatusCode = statusCode
                    };
                }
                return;
            }

            // Handle status code only results (like OkResult/NoContent/Unauthorized/NotFound etc.)
            if (context.Result is StatusCodeResult statusCodeResult)
            {
                var code = statusCodeResult.StatusCode;
                if (code >= 200 && code < 300)
                {
                    context.Result = new ObjectResult(ApiResponse<object>.SuccessResult(null))
                    {
                        StatusCode = 200
                    };
                }
                else
                {
                    string message = code switch
                    {
                        400 => "Bad Request",
                        401 => "Unauthorized",
                        403 => "Forbidden",
                        404 => "Resource Not Found",
                        409 => "Conflict",
                        422 => "Unprocessable Entity",
                        500 => "Internal Server Error",
                        _ => "Operation Failed"
                    };
                    context.Result = new ObjectResult(ApiResponse<object>.ErrorResult(message))
                    {
                        StatusCode = code
                    };
                }
                return;
            }

            // Handle empty results
            if (context.Result is EmptyResult)
            {
                context.Result = new ObjectResult(ApiResponse<object>.SuccessResult(null))
                {
                    StatusCode = 200
                };
                return;
            }

            // Handle text content results
            if (context.Result is ContentResult contentResult)
            {
                context.Result = new ObjectResult(ApiResponse<object>.SuccessResult(contentResult.Content))
                {
                    StatusCode = contentResult.StatusCode ?? 200
                };
                return;
            }
        }
    }

    /// <summary>
    /// Rate limiting filter
    /// </summary>
    public class RateLimitFilter : ActionFilterAttribute
    {
        private readonly int _maxRequests;
        private readonly TimeSpan _timeWindow;
        private static readonly Dictionary<string, List<DateTime>> _requestHistory = new();
        private static readonly object _lock = new object();

        public RateLimitFilter(int maxRequests = 100, int timeWindowMinutes = 1)
        {
            _maxRequests = maxRequests;
            _timeWindow = TimeSpan.FromMinutes(timeWindowMinutes);
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var clientId = GetClientIdentifier(context.HttpContext);
            var now = DateTime.UtcNow;

            lock (_lock)
            {
                if (!_requestHistory.ContainsKey(clientId))
                {
                    _requestHistory[clientId] = new List<DateTime>();
                }

                var requests = _requestHistory[clientId];
                
                // Clean up expired request records
                requests.RemoveAll(time => now - time > _timeWindow);

                if (requests.Count >= _maxRequests)
                {
                    var response = ApiResponse<object>.ErrorResult(
                        "Too many requests", 
                        $"Maximum {_maxRequests} requests allowed per {_timeWindow.TotalMinutes} minutes"
                    );
                    
                    context.Result = new ObjectResult(response)
                    {
                        StatusCode = 429 // Too Many Requests
                    };
                    return;
                }

                requests.Add(now);
            }
        }

        private string GetClientIdentifier(HttpContext context)
        {
            // Prefer user ID, fallback to IP address
            var userId = context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(userId))
            {
                return $"user:{userId}";
            }

            var ipAddress = context.Connection.RemoteIpAddress?.ToString();
            return $"ip:{ipAddress ?? "unknown"}";
        }
    }

    /// <summary>
    /// Input sanitization filter
    /// </summary>
    public class InputSanitizationFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            foreach (var argument in context.ActionArguments.Values)
            {
                if (argument != null)
                {
                    SanitizeObject(argument);
                }
            }
        }

        private void SanitizeObject(object obj)
        {
            if (obj == null) return;

            // Avoid reflection cleanup on JsonElement (contains indexers/internal structures that may throw exceptions on reflection access)
            if (obj is System.Text.Json.JsonElement)
            {
                return;
            }

            var type = obj.GetType();
            
            // Handle string properties (skip indexer properties)
            var stringProperties = type.GetProperties()
                .Where(p => p.GetIndexParameters().Length == 0 && p.PropertyType == typeof(string) && p.CanWrite && p.CanRead);

            foreach (var property in stringProperties)
            {
                var value = property.GetValue(obj) as string;
                if (!string.IsNullOrEmpty(value))
                {
                    // Basic HTML tag cleanup
                    var sanitized = SanitizeString(value);
                    property.SetValue(obj, sanitized);
                }
            }

            // Recursively handle complex objects (skip indexer properties)
            var complexProperties = type.GetProperties()
                .Where(p => p.GetIndexParameters().Length == 0 &&
                           !p.PropertyType.IsPrimitive && 
                           p.PropertyType != typeof(string) && 
                           p.PropertyType != typeof(DateTime) &&
                           p.PropertyType != typeof(decimal) &&
                           p.CanRead);

            foreach (var property in complexProperties)
            {
                object? value;
                try
                {
                    value = property.GetValue(obj);
                }
                catch
                {
                    // Safety fallback: some properties may require parameters or throw exceptions during reflection access, skip them directly
                    continue;
                }

                if (value != null)
                {
                    if (value is System.Collections.IEnumerable enumerable && !(value is string))
                    {
                        foreach (var item in enumerable)
                        {
                            if (item == null) continue;
                            if (item is System.Text.Json.JsonElement) continue; // Avoid recursion on JsonElement
                            SanitizeObject(item);
                        }
                    }
                    else
                    {
                        SanitizeObject(value);
                    }
                }
            }
        }

        private string SanitizeString(string input)
        {
            // Non-strict cleanup: remove basic HTML tags
            return System.Text.RegularExpressions.Regex.Replace(input, "<.*?>", string.Empty);
        }
    }
}