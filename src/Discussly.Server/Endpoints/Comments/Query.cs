using Discussly.Server.DTO;
using Discussly.Server.DTO.Requests;
using Discussly.Server.DTO.Responses;
using Discussly.Server.Exceptions;
using Discussly.Server.Services.Queries.Comments;
using HotChocolate.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Discussly.Server.Endpoints.Comments
{
    [Authorize]
    public class Query(IMediator mediator, ILogger<Query> logger)
    {
        public async Task<PagedCommentsResponse> GetLatestComments(
            [FromBody] GetCommentsRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = new GetLatestCommentsQuery(request);
                var response = await mediator.Send(query, cancellationToken);

                return response;
            }
            catch (ForbiddenException ex)
            {
                logger.LogError(message: ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<Comment> GetCommentById(
            [FromBody] GetCommentByIdRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = new GetCommentByIdQuery(request);
                var response = await mediator.Send(query, cancellationToken);

                return response;
            }
            catch (ForbiddenException ex)
            {
                logger.LogError(message: ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred: {ErrorMessage}", ex.Message);
                throw;
            }
        }
    }
}