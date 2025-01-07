namespace Discussly.Server.DTO.Responses
{
    public class AuthenticationResult
    {
        public bool Succeeded { get; set; }

        public IEnumerable<string> Errors { get; set; } = [];

        public string Token { get; set; } = default!;

        public string? Username { get; set; }

        public string? RedirectUrl { get; set; }

        public int? RoleId { get; set; }

        public static AuthenticationResult CreateErrorResult(params string[] errors)
        {
            return new AuthenticationResult
            {
                Succeeded = false,
                Errors = errors
            };
        }

        public static AuthenticationResult Ok()
        {
            return new AuthenticationResult { Succeeded = true };
        }
    }
}