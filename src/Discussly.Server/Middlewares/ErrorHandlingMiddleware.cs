using Discussly.Server.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text.Json;

namespace Discussly.Server.Middlewares
{
    public class ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger) : IMiddleware
    {
        private static readonly Dictionary<int, string> StatusMessages = new()
        {
            { StatusCodes.Status400BadRequest, "Your request cannot be processed due to incorrect format. Please check the entered information and try again." },
            { StatusCodes.Status401Unauthorized, "You are not authorized. Please log in to access this resource." },
            { StatusCodes.Status403Forbidden, "You do not have permission to access this resource." },
            { StatusCodes.Status404NotFound, "The requested page was not found. Please check the URL and try again." },
            { StatusCodes.Status422UnprocessableEntity, "Some of the information provided is incorrect or incomplete. Please correct the data and try again." },
            { StatusCodes.Status500InternalServerError, "An internal server error has occurred. We are working to eliminate it. Please try again later." }
        };

        private static string? errorMessage;

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
                await HandleStatusCodeAsync(context);
            }
            catch (WebSocketException)
            {
                if (context.WebSockets.IsWebSocketRequest && context.WebSockets.WebSocketRequestedProtocols.Count > 0)
                {
                    var socket = await context.WebSockets.AcceptWebSocketAsync();
                    await socket.CloseAsync(WebSocketCloseStatus.InternalServerError, "WebSocket error", CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleStatusCodeAsync(HttpContext context)
        {
            if (context.Response.HasStarted || !StatusMessages.TryGetValue(context.Response.StatusCode, out var message))
                return;

            errorMessage = message;
            await WriteResponseAsync(context, message);
        }

        private static async Task WriteResponseAsync(HttpContext context, string message)
        {
            context.Response.ContentType = "application/json";

            var result = JsonSerializer.Serialize(new { error = message });
            await context.Response.WriteAsync(result);
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = exception switch
            {
                NoContentException => StatusCodes.Status204NoContent,
                BadRequestException => StatusCodes.Status400BadRequest,
                ForbiddenException => StatusCodes.Status403Forbidden,
                NotFoundException => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            };

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = errorMessage,
                Type = StatusMessages.TryGetValue(statusCode, out var title) ? title : "An unexpected error occurred",
                Instance = context.Request.Path
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, Configuration.JsonOptions.DefaultJsonSerializerOptions));
        }
    }
}