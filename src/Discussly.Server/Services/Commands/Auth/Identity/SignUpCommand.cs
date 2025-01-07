using Discussly.Server.DTO.Responses;
using Discussly.Server.Endpoints.Auth.Identity;
using MediatR;

namespace Discussly.Server.Services.Commands.Auth.Identity
{
    public class SignUpCommand(SignUpRequest model) : IRequest<AuthenticationResult>
    {
        public SignUpRequest Model { get; } = model;
    }
}