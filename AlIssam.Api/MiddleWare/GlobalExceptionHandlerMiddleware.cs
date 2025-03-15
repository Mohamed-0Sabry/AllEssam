using AlIssam.API.Dtos.Response;
using AlIssam.API.Extension;
using System.Text.Json;

namespace AlIssam.Api.MiddleWare
{
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
            //catch (SqlException sqlEx) when (sqlEx.Number == 2627 || sqlEx.Number == 2601) // Unique constraint violation
            //{
            //    var errorResponse = new ErrorResponse().CreateErrorResponse(
            //   "Something went wrong. Please contact the owners if the issue persists.",
            //   "حدث خطأ ما. يرجى الاتصال بالمالكين إذا استمرت المشكلة."
            //   );

            //    await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));

            //}
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception has occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var errorResponse = new ErrorResponse().CreateErrorResponse(
                ex.Message,
                "حدث خطأ ما. يرجى الاتصال بالمالكين إذا استمرت المشكلة."
            );


            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
        }
    }
}
