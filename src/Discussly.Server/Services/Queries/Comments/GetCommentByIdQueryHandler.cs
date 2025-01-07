using Discussly.Server.Data.Domain.Interfaces;
using Discussly.Server.DTO;
using Discussly.Server.SharedKernel.DTO;
using MediatR;

namespace Discussly.Server.Services.Queries.Comments
{
    public class GetCommentByIdQueryHandler(IDiscussionDataUnitOfWork unitOfWork)
        : IRequestHandler<GetCommentByIdQuery, Comment>
    {
        public async Task<Comment> Handle(GetCommentByIdQuery request, CancellationToken cancellationToken)
        {
            var comment = await unitOfWork.Comments.GetCommentByIdAsync(request.Model.Id, cancellationToken);
            return MapperService.Map<CommentDto, Comment>(comment);
        }
    }
}