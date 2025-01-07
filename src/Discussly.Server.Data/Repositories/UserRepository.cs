using Discussly.Server.Data.Domain;
using Discussly.Server.Data.Entities.Users;
using Discussly.Server.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Discussly.Server.Data.Repositories
{
    public class UserRepository(DiscussionDataContext discussionDataContext) : IUserRepository
    {
        public async Task<bool> IsUserExistsByEmailAsync(string email, CancellationToken cancellationToken)
        {
            var userId = await discussionDataContext.Users
                .Where(u => u.Email == email)
                .Select(u => u.Id)
                .FirstOrDefaultAsync(cancellationToken);

            return !string.IsNullOrEmpty(userId);
        }

        public async Task<bool> IsUserExistsByIdAsync(string id, CancellationToken cancellationToken)
        {
            var userId = await discussionDataContext.Users
               .Where(u => u.Id == id)
               .Select(u => u.Id)
               .FirstOrDefaultAsync(cancellationToken);

            return !string.IsNullOrEmpty(userId);
        }

        public async Task<bool> IsUserExistsByUserNameAsync(string userName, CancellationToken cancellationToken)
        {
            var userId = await discussionDataContext.Users
                .Where(u => u.UserName == userName)
                .Select(u => u.Id)
                .FirstOrDefaultAsync(cancellationToken);

            return !string.IsNullOrEmpty(userId);
        }

        public async Task<UserInfo> GetUserInfoAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await discussionDataContext.Users
                .Where(u => u.Id == userId)
                .Select(u => new UserInfo
                {
                    Email = u.Email!,
                    Username = u.UserName!,
                    AvatarUrl = u.AvatarUrl,
                    HomePage = u.HomePage,
                })
                .FirstAsync(cancellationToken);
        }

        public async Task<User> GetUserByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return await discussionDataContext.Users
                .Where(u => u.Id == id)
                .FirstAsync(cancellationToken);
        }

        public async Task UpdateUserInfoAsync(User user, CancellationToken cancellationToken)
        {
            await discussionDataContext.Users
                .Where(u => u.Id == user.Id)
                .ExecuteUpdateAsync(st => st
                    .SetProperty(p => p.HomePage, user.HomePage),
                    cancellationToken);
        }

        public async Task UpdateUserAvatarAsync(string id, string avatarUrl, CancellationToken cancellationToken = default)
        {
            await discussionDataContext.Users
               .Where(u => u.Id == id)
               .ExecuteUpdateAsync(st => st
                   .SetProperty(p => p.AvatarUrl, avatarUrl),
                   cancellationToken);
        }
    }
}