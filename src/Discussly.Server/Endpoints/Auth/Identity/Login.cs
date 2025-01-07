using Ardalis.ApiEndpoints;
using Discussly.Server.Exceptions;
using Discussly.Server.Services.Commands.Auth.Identity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Discussly.Server.Endpoints.Auth.Identity
{
    public class Login(IMediator mediator, ILogger<Login> logger) : EndpointBaseAsync
        .WithRequest<LoginRequest>
        .WithActionResult
    {
        [HttpPost("account/login")]
        public override async Task<ActionResult> HandleAsync([FromBody] LoginRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var command = new LoginCommand(request);
                var result = await mediator.Send(command, cancellationToken);

                if (result.Succeeded)
                    return Ok(new { result.Username, result.RoleId, result.Token });

                return BadRequest(new { result.Errors });
            }
            catch (NotFoundException ex)
            {
                logger.LogError(ex, "An unexpected error occurred: {ErrorMessage}", ex.StackTrace);
                return NotFound();
            }
            catch (InvalidPasswordException ex)
            {
                logger.LogError(ex, "An unexpected error occurred: {ErrorMessage}", ex.StackTrace);
                return Unauthorized();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred: {ErrorMessage}", ex.StackTrace);
                throw;
            }
        }
    }
}