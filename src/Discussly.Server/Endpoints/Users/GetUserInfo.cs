using Ardalis.ApiEndpoints;
using Discussly.Server.Services.Queries.Users;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Discussly.Server.Endpoints.Users
{
    public class GetUserInfo(IMediator mediator, ILogger<GetUserInfo> logger) : EndpointBaseAsync
        .WithoutRequest
        .WithActionResult
    {
        [HttpGet("userInfo")]
        public override async Task<ActionResult> HandleAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var command = new GetUserInfoQuery();
                var response = await mediator.Send(command, cancellationToken);

                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred: {ErrorMessage}", ex.Message);
                throw;
            }
        }
    }
}