using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Discussly.Server.Data.Entities.Users
{
    public class User : IdentityUser
    {
        [MaxLength(100)]
        [DefaultValue("")]
        public string Name { get; set; } = default!;

        public string? HomePage { get; set; }

        public string? AvatarUrl { get; set; }

        public DateTime CreatedDate { get; set; } = default!;
    }
}