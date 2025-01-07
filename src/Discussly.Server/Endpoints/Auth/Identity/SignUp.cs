using Ardalis.ApiEndpoints;
using Discussly.Server.Exceptions;
using Discussly.Server.Services.Commands.Auth.Identity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Discussly.Server.Endpoints.Auth.Identity
{
    public class SignUp(IMediator mediator, ILogger<SignUp> logger) : EndpointBaseAsync
        .WithRequest<SignUpRequest>
        .WithActionResult
    {
        [HttpPost("account/sign-up")]
        public override async Task<ActionResult> HandleAsync([FromBody] SignUpRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var command = new SignUpCommand(request);
                var result = await mediator.Send(command, cancellationToken);

                if (result.Succeeded)
                    return Ok();

                return BadRequest(new { result.Errors });
            }
            catch (InvalidCaptchaException ex)
            {
                logger.LogError(ex, "Invalid CAPTCHA entered by the user.");

                return BadRequest(new { message = "The entered CAPTCHA is incorrect. Please try again." });
            }
            catch (ConflictException ex)
            {
                logger.LogError(ex, "A user with this email or username already exists.");
                return Conflict();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred: {ErrorMessage}", ex.Message);
                throw;
            }
        }
    }
}