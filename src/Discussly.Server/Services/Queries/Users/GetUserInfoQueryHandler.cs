using Discussly.Server.Data.Domain.Interfaces;
using Discussly.Server.DTO.Users;
using Discussly.Server.Exceptions;
using MediatR;
using System.Security.Claims;

namespace Discussly.Server.Services.Queries.Users
{
    public class GetUserInfoQueryHandler(
        IHttpContextAccessor httpContextAccessor,
        IDiscussionDataUnitOfWork discussionDataUnitOfWork
    ) : IRequestHandler<GetUserInfoQuery, UserInfo>
    {
        public async Task<UserInfo> Handle(GetUserInfoQuery request, CancellationToken cancellationToken)
        {
            var claims = httpContextAccessor.HttpContext!.User.Claims;
            var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                ?? throw new ForbiddenException("User not found");

            var userInfoEntity = await discussionDataUnitOfWork.Users.GetUserInfoAsync(userId, cancellationToken);

            var userInfo = MapperService.Map<Data.Entities.Users.UserInfo, UserInfo>(userInfoEntity);

            return userInfo;
        }
    }
}