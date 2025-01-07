using Discussly.Server.DTO.Responses;
using Discussly.Server.Endpoints.Auth.Identity;
using MediatR;

namespace Discussly.Server.Services.Commands.Auth.Identity
{
    public class LoginCommand(LoginRequest model) : IRequest<AuthenticationResult>
    {
        public LoginRequest Model { get; } = model;
    }
}