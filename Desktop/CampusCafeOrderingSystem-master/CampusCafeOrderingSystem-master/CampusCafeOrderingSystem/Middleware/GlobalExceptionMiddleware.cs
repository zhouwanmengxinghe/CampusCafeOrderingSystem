using CampusCafeOrderingSystem.Models.DTOs;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace CampusCafeOrderingSystem.Middleware
{
    /// <summary>
    /// Global exception handling middleware
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred. Request: {Method} {Path}", 
                    context.Request.Method, context.Request.Path);

                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new ApiResponse<object>();
            
            switch (exception)
            {
                case ValidationException validationEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = ApiResponse<object>.ErrorResult("Data validation failed", validationEx.Message);
                    break;

                case ArgumentNullException nullEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = ApiResponse<object>.ErrorResult("Required parameter missing", nullEx.ParamName ?? "Unknown parameter");
                    break;

                case ArgumentException argEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = ApiResponse<object>.ErrorResult("Invalid request parameter", argEx.Message);
                    break;

                case UnauthorizedAccessException:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response = ApiResponse<object>.ErrorResult("Unauthorized access", "You do not have permission to perform this operation");
                    break;

                case KeyNotFoundException:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response = ApiResponse<object>.ErrorResult("Resource not found", "The requested resource does not exist");
                    break;

                case InvalidOperationException invalidOpEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = ApiResponse<object>.ErrorResult("Invalid operation", invalidOpEx.Message);
                    break;

                case TimeoutException:
                    context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                    response = ApiResponse<object>.ErrorResult("Request timeout", "Server processing request timed out, please try again later");
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    
                    if (_environment.IsDevelopment())
                    {
                        response = ApiResponse<object>.ErrorResult("Internal server error", new List<string>
                        {
                            exception.Message,
                            exception.StackTrace ?? "No stack trace information"
                        });
                    }
                    else
                    {
                        response = ApiResponse<object>.ErrorResult("Internal server error", "The server encountered an error, please try again later");
                    }
                    break;
            }

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }

    /// <summary>
    /// Custom validation exception
    /// </summary>
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
        public ValidationException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Business logic exception
    /// </summary>
    public class BusinessException : Exception
    {
        public string ErrorCode { get; }
        
        public BusinessException(string message) : base(message) 
        {
            ErrorCode = "BUSINESS_ERROR";
        }
        
        public BusinessException(string errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }
        
        public BusinessException(string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = "BUSINESS_ERROR";
        }
    }

    /// <summary>
    /// Resource not found exception
    /// </summary>
    public class ResourceNotFoundException : Exception
    {
        public string ResourceType { get; }
        public string ResourceId { get; }
        
        public ResourceNotFoundException(string resourceType, string resourceId) 
            : base($"{resourceType} with ID '{resourceId}' was not found")
        {
            ResourceType = resourceType;
            ResourceId = resourceId;
        }
    }

    /// <summary>
    /// Insufficient permissions exception
    /// </summary>
    public class InsufficientPermissionsException : Exception
    {
        public string RequiredPermission { get; }
        
        public InsufficientPermissionsException(string requiredPermission) 
            : base($"Insufficient permissions. Required: {requiredPermission}")
        {
            RequiredPermission = requiredPermission;
        }
    }
}