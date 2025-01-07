using Ardalis.ApiEndpoints;
using Discussly.Server.Exceptions;
using Discussly.Server.Services.Commands.Users;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Discussly.Server.Endpoints.Users
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UpdateUserInfo(IMediator mediator, ILogger<UpdateUserInfo> logger) : EndpointBaseAsync
        .WithRequest<UpdateUserInfoRequest>
        .WithActionResult
    {
        [HttpPut("userInfo")]
        public override async Task<ActionResult> HandleAsync([FromForm] UpdateUserInfoRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var command = new UpdateUserInfoCommand(request);
                await mediator.Send(command, cancellationToken);

                return Ok();
            }
            catch (ForbiddenException ex)
            {
                logger.LogError(message: ex.Message);
                return new ForbidResult();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred: {ErrorMessage}", ex.Message);
                throw;
            }
        }
    }
}