using Discussly.Server.Endpoints.Comments;
using MediatR;

namespace Discussly.Server.Services.Commands.Comments
{
    public class AddCommentCommand(AddCommentRequest model) : IRequest
    {
        public AddCommentRequest Model { get; } = model;
    }
}