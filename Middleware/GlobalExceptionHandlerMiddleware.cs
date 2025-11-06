using System.Net;
using System.Text.Json;

namespace UniStart.Middleware
{
    /// <summary>
    /// Middleware для глобальной обработки исключений
    /// </summary>
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            var statusCode = HttpStatusCode.InternalServerError;
            var message = "Произошла внутренняя ошибка сервера";
            var details = exception.Message;

            // Определяем тип ошибки и соответствующий HTTP статус
            switch (exception)
            {
                case ArgumentNullException _:
                case ArgumentException _:
                    statusCode = HttpStatusCode.BadRequest;
                    message = "Неверный запрос";
                    break;

                case UnauthorizedAccessException _:
                    statusCode = HttpStatusCode.Unauthorized;
                    message = "Доступ запрещен";
                    break;

                case KeyNotFoundException _:
                    statusCode = HttpStatusCode.NotFound;
                    message = "Ресурс не найден";
                    break;

                case InvalidOperationException invalidOpEx when invalidOpEx.Message.Contains("Sequence contains no elements"):
                    statusCode = HttpStatusCode.NotFound;
                    message = "Ресурс не найден";
                    break;
            }

            context.Response.StatusCode = (int)statusCode;

            var response = new ErrorResponse
            {
                StatusCode = (int)statusCode,
                Message = message,
                Details = details,
                Timestamp = DateTime.UtcNow
            };

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
        }
    }

    /// <summary>
    /// Модель ответа для ошибок
    /// </summary>
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
