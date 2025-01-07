using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Discussly.Server.Endpoints.Users
{
    public class UpdateUserInfoRequest
    {
        [FromForm]
        public IFormFile? FileStream { get; set; }

        [FromForm]
        public string? HomePage { get; set; }

        [AllowedValues("None", "Update", "Delete")]
        public string AvatarAction { get; set; } = default!;
    }
}