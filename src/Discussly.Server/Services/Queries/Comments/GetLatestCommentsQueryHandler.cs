using Discussly.Server.Data.Domain.Interfaces;
using Discussly.Server.DTO;
using Discussly.Server.DTO.Responses;
using Discussly.Server.Infrastructure.Services.Elastic.Interfaces;
using Discussly.Server.SharedKernel.DTO;
using MediatR;

namespace Discussly.Server.Services.Queries.Comments
{
    public class GetLatestCommentsQueryHandler(
        IDiscussionDataUnitOfWork unitOfWork,
        IElasticSearchService elasticSearchService
    )
        : IRequestHandler<GetLatestCommentsQuery, PagedCommentsResponse>
    {
        public async Task<PagedCommentsResponse> Handle(GetLatestCommentsQuery request, CancellationToken cancellationToken)
        {
            var rootCommentsIds = await elasticSearchService.GetRootCommentsAsync(
                request.Model.PageIndex,
                request.Model.PageSize,
                request.Model.SearchText,
                request.Model.SortDirection,
                request.Model.SortBy,
                cancellationToken);

            var comments = await unitOfWork.Comments.GetLatestCommentsAsync(rootCommentsIds, cancellationToken);

            var mappedComments = MapperService.Map<CommentDto[], Comment[]>(comments);

            var totalComments = await elasticSearchService.GetTotalCountAsync(cancellationToken);

            return new PagedCommentsResponse
            {
                Comments = mappedComments,
                TotalComments = totalComments,
                CurrentPage = request.Model.PageIndex
            };
        }
    }
}