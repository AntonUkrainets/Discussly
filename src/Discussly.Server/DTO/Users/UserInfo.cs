namespace Discussly.Server.DTO.Users
{
    public class UserInfo
    {
        public string Email { get; set; } = default!;

        public string Username { get; set; } = default!;

        public string? AvatarUrl { get; set; }

        public string? HomePage { get; set; }
    }
}