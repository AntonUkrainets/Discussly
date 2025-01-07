using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Discussly.Server.Endpoints.Auth.Identity
{
    public class SignUpRequest
    {
        [FromBody]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(254, MinimumLength = 5, ErrorMessage = "Email must be between 5 and 254 characters.")]
        public string Email { get; set; } = default!;

        [FromBody]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 30 characters.")]
        public string Username { get; set; } = default!;

        [FromBody]
        [StringLength(64, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 64 characters.")]
        public string Password { get; set; } = default!;

        [FromBody]
        [Url(ErrorMessage = "Invalid URL format.")]
        public string? HomePage { get; set; }

        [FromBody]
        [StringLength(maximumLength: 6, MinimumLength = 6, ErrorMessage = "Missing CAPTCHA code.")]
        public string CaptchaCode { get; set; } = default!;
    }
}