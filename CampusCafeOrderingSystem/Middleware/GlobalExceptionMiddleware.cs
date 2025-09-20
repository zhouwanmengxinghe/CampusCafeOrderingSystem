using CampusCafeOrderingSystem.Models.DTOs;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace CampusCafeOrderingSystem.Middleware
{
    /// <summary>
    /// 全局异常处理中间件
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
                    response = ApiResponse<object>.ErrorResult("数据验证失败", validationEx.Message);
                    break;

                case ArgumentNullException nullEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = ApiResponse<object>.ErrorResult("必需参数缺失", nullEx.ParamName ?? "未知参数");
                    break;

                case ArgumentException argEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = ApiResponse<object>.ErrorResult("请求参数无效", argEx.Message);
                    break;

                case UnauthorizedAccessException:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response = ApiResponse<object>.ErrorResult("未授权访问", "您没有权限执行此操作");
                    break;

                case KeyNotFoundException:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response = ApiResponse<object>.ErrorResult("资源未找到", "请求的资源不存在");
                    break;

                case InvalidOperationException invalidOpEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = ApiResponse<object>.ErrorResult("操作无效", invalidOpEx.Message);
                    break;

                case TimeoutException:
                    context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                    response = ApiResponse<object>.ErrorResult("请求超时", "服务器处理请求超时，请稍后重试");
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    
                    if (_environment.IsDevelopment())
                    {
                        response = ApiResponse<object>.ErrorResult("服务器内部错误", new List<string>
                        {
                            exception.Message,
                            exception.StackTrace ?? "无堆栈跟踪信息"
                        });
                    }
                    else
                    {
                        response = ApiResponse<object>.ErrorResult("服务器内部错误", "服务器遇到了一个错误，请稍后重试");
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
    /// 自定义验证异常
    /// </summary>
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
        public ValidationException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// 业务逻辑异常
    /// </summary>
    public class BusinessException : Exception
    {
        public string ErrorCode { get; }

        public BusinessException(string message, string errorCode = "BUSINESS_ERROR") : base(message)
        {
            ErrorCode = errorCode;
        }

        public BusinessException(string message, Exception innerException, string errorCode = "BUSINESS_ERROR") 
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }

    /// <summary>
    /// 资源不存在异常
    /// </summary>
    public class ResourceNotFoundException : Exception
    {
        public string ResourceType { get; }
        public string ResourceId { get; }

        public ResourceNotFoundException(string resourceType, string resourceId) 
            : base($"{resourceType} with ID '{resourceId}' was not found.")
        {
            ResourceType = resourceType;
            ResourceId = resourceId;
        }
    }

    /// <summary>
    /// 权限不足异常
    /// </summary>
    public class InsufficientPermissionException : Exception
    {
        public string RequiredPermission { get; }

        public InsufficientPermissionException(string requiredPermission) 
            : base($"Insufficient permission. Required: {requiredPermission}")
        {
            RequiredPermission = requiredPermission;
        }
    }
}