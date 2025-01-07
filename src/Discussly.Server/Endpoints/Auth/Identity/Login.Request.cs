using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Discussly.Server.Endpoints.Auth.Identity
{
    public sealed class LoginRequest
    {
        [FromBody]
        [EmailAddress]
        [Length(5, 254)]
        public string Email { get; set; } = default!;

        [FromBody]
        [Length(8, 64)]
        public string Password { get; set; } = default!;
    }
}