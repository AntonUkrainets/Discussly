using Discussly.Server.Data.Domain.Interfaces;
using Discussly.Server.Data.Entities.Users;
using Discussly.Server.DTO.Responses;
using Discussly.Server.DTO.Users;
using Discussly.Server.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Data;
using System.Security.Claims;

namespace Discussly.Server.Services.Commands.Auth.Identity
{
    public class SignUpCommandHandler(UserManager<User> userManager, IDiscussionDataUnitOfWork discussionDataUnitOfWork)
        : IRequestHandler<SignUpCommand, AuthenticationResult>
    {
        public async Task<AuthenticationResult> Handle(SignUpCommand request, CancellationToken cancellationToken)
        {
            var emailAlreadyExists = await discussionDataUnitOfWork.Users.IsUserExistsByEmailAsync(request.Model.Email, cancellationToken);

            if (emailAlreadyExists)
                throw new ConflictException(request.Model.Email);

            var usernameAlreadyExists = await discussionDataUnitOfWork.Users.IsUserExistsByUserNameAsync(request.Model.Username, cancellationToken);

            if (usernameAlreadyExists)
                throw new ConflictException(request.Model.Username);

            var isCaptchaValid = await IsCaptchaValidAsync(request.Model.Email, request.Model.CaptchaCode, cancellationToken);

            if (!isCaptchaValid)
                throw new InvalidCaptchaException();

            var utcNow = DateTime.UtcNow;

            var newUser = new User
            {
                Name = request.Model.Username,
                UserName = request.Model.Username,
                Email = request.Model.Email,
                HomePage = request.Model.HomePage,
                CreatedDate = utcNow,
            };

            var createResult = await userManager.CreateAsync(newUser, request.Model.Password);
            if (!createResult.Succeeded)
            {
                return AuthenticationResult.CreateErrorResult(
                    [.. createResult.Errors.Select(e => e.Description)]);
            }

            await userManager.AddToRoleAsync(newUser, Role.User);

            var claims = new List<Claim>
            {
                new (ClaimTypes.NameIdentifier, newUser.Id),
                new (ClaimTypes.Name, newUser.Name),
                new (ClaimTypes.Email, newUser.Email),
                new (ClaimTypes.Role, Role.User),
            };

            var addClaimsResult = await userManager.AddClaimsAsync(newUser, claims);
            if (!addClaimsResult.Succeeded)
            {
                return AuthenticationResult.CreateErrorResult([.. createResult.Errors.Select(e => e.Description)]);
            }

            return AuthenticationResult.Ok();
        }

        private async Task<bool> IsCaptchaValidAsync(string userEmail, string inputCaptcha, CancellationToken cancellationToken)
        {
            var captchaCode = await discussionDataUnitOfWork.Sessions.GetCurrencySessionAsync(userEmail, cancellationToken);

            if (captchaCode == inputCaptcha)
                return true;

            return false;
        }
    }
}