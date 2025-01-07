using Discussly.Server.DTO;
using Discussly.Server.DTO.Requests;
using MediatR;

namespace Discussly.Server.Services.Queries.Comments
{
    public class GetCommentByIdQuery(GetCommentByIdRequest request) : IRequest<Comment>
    {
        public GetCommentByIdRequest Model { get; } = request;
    }
}