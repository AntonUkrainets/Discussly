namespace Discussly.Server.Data.Entities.Users
{
    public class UserInfo
    {
        public string? AvatarUrl { get; set; }

        public string Email { get; set; } = default!;

        public string Username { get; set; } = default!;

        public string? HomePage { get; set; }
    }
}