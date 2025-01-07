using Discussly.Server.Endpoints.Users;
using MediatR;

namespace Discussly.Server.Services.Commands.Users
{
    public class UpdateUserInfoCommand(UpdateUserInfoRequest model) : IRequest
    {
        public UpdateUserInfoRequest Model { get; } = model;
    }
}