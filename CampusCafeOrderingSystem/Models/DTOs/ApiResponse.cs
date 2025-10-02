using System.ComponentModel.DataAnnotations;

namespace CampusCafeOrderingSystem.Models.DTOs
{
    /// <summary>
    /// 标准API响应格式
    /// </summary>
    /// <typeparam name="T">响应数据类型</typeparam>
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiResponse<T> SuccessResult(T data, string message = "操作成功")
        {
            return new ApiResponse<T>
            {
                IsSuccess = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> ErrorResult(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }

        public static ApiResponse<T> ErrorResult(string message, string error)
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                Message = message,
                Errors = new List<string> { error }
            };
        }

        // 添加简化的静态方法以匹配控制器中的使用
        public static ApiResponse<T> Success(T data, string message = "操作成功")
        {
            return SuccessResult(data, message);
        }

        public static ApiResponse<T> Error(string message, int statusCode = 400)
        {
            return ErrorResult(message);
        }

        public static ApiResponse<T> Error(string message, List<string> errors)
        {
            return ErrorResult(message, errors);
        }
    }

    /// <summary>
    /// 分页响应格式
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public class PagedApiResponse<T> : ApiResponse<IEnumerable<T>>
    {
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public bool HasNextPage => CurrentPage < TotalPages;
        public bool HasPreviousPage => CurrentPage > 1;

        public static PagedApiResponse<T> SuccessResult(IEnumerable<T> data, int totalCount, int currentPage, int pageSize, string message = "操作成功")
        {
            return new PagedApiResponse<T>
            {
                IsSuccess = true,
                Message = message,
                Data = data,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                CurrentPage = currentPage,
                PageSize = pageSize
            };
        }
    }

    /// <summary>
    /// 分页结果类
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public bool HasNextPage => CurrentPage < TotalPages;
        public bool HasPreviousPage => CurrentPage > 1;

        public PagedResult()
        {
        }

        public PagedResult(List<T> items, int totalCount, int currentPage, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        }
    }
}