using System.Net;

namespace Discussly.Server.DTO.Responses
{
    public class OperationResult
    {
        public HttpStatusCode StatusCode { get; init; }

        public object? Response { get; init; }

        private OperationResult()
        {
        }

        public static OperationResult Ok(object? data = default) => new() { StatusCode = HttpStatusCode.OK, Response = data };

        public static OperationResult NotFound() => new() { StatusCode = HttpStatusCode.NotFound };

        public static OperationResult BadRequest(object? data) => new() { StatusCode = HttpStatusCode.BadRequest, Response = data };

        public static OperationResult Forbidden() => new() { StatusCode = HttpStatusCode.Forbidden };

        public static OperationResult NoContent() => new() { StatusCode = HttpStatusCode.NoContent };

        public static OperationResult Conflict() => new() { StatusCode = HttpStatusCode.Conflict };
    }
}