using Ardalis.ApiEndpoints;
using Discussly.Server.Services.Queries.Captcha;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Discussly.Server.Endpoints.Captcha
{
    public class GetCaptcha(IMediator mediator, ILogger<GetCaptcha> logger) : EndpointBaseAsync
        .WithRequest<GetCaptchaRequest>
        .WithActionResult
    {
        [HttpGet("captcha")]
        public override async Task<ActionResult> HandleAsync([FromQuery] GetCaptchaRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var command = new GetCaptchaQuery(request);
                var response = await mediator.Send(command, cancellationToken);

                return File(response.Image, "image/png");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred: {ErrorMessage}", ex.Message);
                throw;
            }
        }
    }
}