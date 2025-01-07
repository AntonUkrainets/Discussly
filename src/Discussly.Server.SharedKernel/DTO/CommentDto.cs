namespace Discussly.Server.SharedKernel.DTO
{
    public class CommentDto
    {
        public Guid Id { get; set; }

        public Guid? RootCommentId { get; set; }

        public Guid? ParentCommentId { get; set; }

        public string Text { get; set; } = default!;

        public string? AttachmentFileName { get; set; }

        public string? AttachmentUrl { get; set; }

        public DateTime CreatedAt { get; set; }

        public string UserName { get; set; } = default!;

        public string UserId { get; set; } = default!;

        public string? UserAvatar { get; set; }

        public bool HasFile { get; set; }

        public CommentDto[] ChildComments { get; set; } = [];
    }
}