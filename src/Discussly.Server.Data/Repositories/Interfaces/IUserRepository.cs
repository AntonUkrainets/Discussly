using Discussly.Server.Data.Entities.Users;

namespace Discussly.Server.Data.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> IsUserExistsByEmailAsync(string email, CancellationToken cancellationToken);

        Task<bool> IsUserExistsByUserNameAsync(string userName, CancellationToken cancellationToken);

        Task<bool> IsUserExistsByIdAsync(string id, CancellationToken cancellationToken);

        Task<UserInfo> GetUserInfoAsync(string userId, CancellationToken cancellationToken= default);

        Task<User> GetUserByIdAsync(string id, CancellationToken cancellationToken= default);

        Task UpdateUserInfoAsync(User user, CancellationToken cancellationToken = default);

        Task UpdateUserAvatarAsync(string id, string avatarUrl, CancellationToken cancellationToken = default);
    }
}