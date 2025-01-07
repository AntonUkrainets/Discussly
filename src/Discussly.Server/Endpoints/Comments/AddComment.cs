using Ardalis.ApiEndpoints;
using Discussly.Server.Exceptions;
using Discussly.Server.Services.Commands.Comments;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Discussly.Server.Endpoints.Comments
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AddComment(IMediator mediator, ILogger<AddComment> logger) : EndpointBaseAsync
        .WithRequest<AddCommentRequest>
        .WithActionResult
    {
        [HttpPost("comments")]
        public override async Task<ActionResult> HandleAsync([FromForm] AddCommentRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var command = new AddCommentCommand(request);
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