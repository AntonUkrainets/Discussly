using Discussly.Server.Data.Entities.Users;

namespace Discussly.Server.Services.Tokens.Interfaces
{
    public interface ITokenService
    {
        Task<string> CreateTokenAsync(User user);
    }
}