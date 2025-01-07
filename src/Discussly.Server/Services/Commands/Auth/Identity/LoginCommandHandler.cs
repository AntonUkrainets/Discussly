using Discussly.Server.Data.Entities.Users;
using Discussly.Server.DTO.Responses;
using Discussly.Server.DTO.Users;
using Discussly.Server.Exceptions;
using Discussly.Server.Services.Tokens.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Discussly.Server.Services.Commands.Auth.Identity
{
    public class LoginCommandHandler(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ITokenService tokenService
    )
        : IRequestHandler<LoginCommand, AuthenticationResult>
    {
        public async Task<AuthenticationResult> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = userManager.Users.FirstOrDefault(u => u.Email == request.Model.Email)
                ?? throw new NotFoundException(request.Model.Email);

            var result = await signInManager.CheckPasswordSignInAsync(user, request.Model.Password, true);
            if (!result.Succeeded)
                throw new InvalidPasswordException();

            await signInManager.SignInAsync(user, true);

            var userRoles = await userManager.GetRolesAsync(user);

            var roleId = Role.DefineRoleId(userRoles);

            var token = await tokenService.CreateTokenAsync(user);

            return new AuthenticationResult
            {
                Succeeded = true,
                Token = token,
                Username = user.UserName,
                RoleId = roleId
            };
        }
    }
}