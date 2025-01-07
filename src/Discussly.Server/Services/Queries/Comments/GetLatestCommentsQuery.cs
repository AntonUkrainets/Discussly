using Discussly.Server.DTO.Requests;
using Discussly.Server.DTO.Responses;
using MediatR;

namespace Discussly.Server.Services.Queries.Comments
{
    public class GetLatestCommentsQuery(GetCommentsRequest request) : IRequest<PagedCommentsResponse>
    {
        public GetCommentsRequest Model { get; } = request;
    }
}