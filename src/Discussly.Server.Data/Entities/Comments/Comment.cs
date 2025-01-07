using Discussly.Server.Data.Entities.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Discussly.Server.Data.Entities.Comments
{
    public class Comment
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [ForeignKey(nameof(User))]
        public string UserId { get; set; } = default!;
        public User User { get; set; } = default!;

        public Guid RootCommentId { get; set; }

        public Guid? ParentCommentId { get; set; }
        public Comment? ParentComment { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Text { get; set; } = default!;

        public string? AttachmentFileName { get; set; }

        public string? AttachmentUrl { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public ICollection<Comment> ChildComments { get; set; } = [];
    }
}