using CampusCafeOrderingSystem.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CampusCafeOrderingSystem.Filters
{
    /// <summary>
    /// 模型验证过滤器
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

                var response = ApiResponse<object>.ErrorResult("数据验证失败", errors);
                
                context.Result = new BadRequestObjectResult(response);
            }
        }
    }

    /// <summary>
    /// API响应格式化过滤器
    /// </summary>
    public class ApiResponseFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            // 跳过视图、文件和重定向等非JSON响应
            if (context.Result is ViewResult ||
                context.Result is FileResult ||
                context.Result is RedirectResult ||
                context.Result is RedirectToActionResult ||
                context.Result is RedirectToRouteResult ||
                context.Result is LocalRedirectResult)
            {
                return;
            }

            // 处理包含对象负载的结果
            if (context.Result is ObjectResult objectResult)
            {
                var statusCode = objectResult.StatusCode ?? 200;
                var value = objectResult.Value;

                // 已经是 ApiResponse<T>，则不再包装
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
                    string message = "操作失败";
                    var errors = new List<string>();

                    if (value is ProblemDetails pd)
                    {
                        message = pd.Title ?? message;
                        if (!string.IsNullOrWhiteSpace(pd.Detail)) errors.Add(pd.Detail);
                    }
                    else if (value is ValidationProblemDetails vpd)
                    {
                        message = vpd.Title ?? "数据验证失败";
                        foreach (var e in vpd.Errors.SelectMany(kv => kv.Value)) errors.Add(e);
                    }
                    else if (value != null)
                    {
                        // 尝试从匿名对象中提取 message / error(s)
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

            // 处理 JsonResult
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
                    context.Result = new ObjectResult(ApiResponse<object>.ErrorResult("操作失败"))
                    {
                        StatusCode = statusCode
                    };
                }
                return;
            }

            // 处理仅有状态码的结果（如 OkResult/NoContent/Unauthorized/NotFound 等）
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
                        400 => "错误的请求",
                        401 => "未授权",
                        403 => "禁止访问",
                        404 => "资源未找到",
                        409 => "冲突",
                        422 => "无法处理的实体",
                        500 => "服务器内部错误",
                        _ => "操作失败"
                    };
                    context.Result = new ObjectResult(ApiResponse<object>.ErrorResult(message))
                    {
                        StatusCode = code
                    };
                }
                return;
            }

            // 处理空结果
            if (context.Result is EmptyResult)
            {
                context.Result = new ObjectResult(ApiResponse<object>.SuccessResult(null))
                {
                    StatusCode = 200
                };
                return;
            }

            // 处理文本内容结果
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
    /// 速率限制过滤器
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
                
                // 清理过期的请求记录
                requests.RemoveAll(time => now - time > _timeWindow);

                if (requests.Count >= _maxRequests)
                {
                    var response = ApiResponse<object>.ErrorResult(
                        "请求过于频繁", 
                        $"每{_timeWindow.TotalMinutes}分钟最多允许{_maxRequests}次请求"
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
            // 优先使用用户ID，其次使用IP地址
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
    /// 输入清理过滤器
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

            // 避免对原始 JsonElement 进行反射清理（其包含索引器/内部结构，反射访问可能触发异常）
            if (obj is System.Text.Json.JsonElement)
            {
                return;
            }

            var type = obj.GetType();
            
            // 处理字符串属性（跳过索引器属性）
            var stringProperties = type.GetProperties()
                .Where(p => p.GetIndexParameters().Length == 0 && p.PropertyType == typeof(string) && p.CanWrite && p.CanRead);

            foreach (var property in stringProperties)
            {
                var value = property.GetValue(obj) as string;
                if (!string.IsNullOrEmpty(value))
                {
                    // 基本的HTML标签清理
                    var sanitized = SanitizeString(value);
                    property.SetValue(obj, sanitized);
                }
            }

            // 递归处理复杂对象（跳过索引器属性）
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
                    // 安全兜底：某些属性在反射读取时可能需要参数或抛出异常，直接跳过
                    continue;
                }

                if (value != null)
                {
                    if (value is System.Collections.IEnumerable enumerable && !(value is string))
                    {
                        foreach (var item in enumerable)
                        {
                            if (item == null) continue;
                            if (item is System.Text.Json.JsonElement) continue; // 避免对 JsonElement 递归
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
            // 非严格清理：移除基本的HTML标签
            return System.Text.RegularExpressions.Regex.Replace(input, "<.*?>", string.Empty);
        }
    }
}