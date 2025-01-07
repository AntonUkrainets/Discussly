namespace Discussly.Server.DTO
{
    public class Comment
    {
        public Guid Id { get; init; }

        public string UserName { get; init; } = default!;

        public string? UserAvatar { get; init; }

        public Guid? ParentCommentId { get; init; }

        public Guid RootCommentId { get; init; }

        public string Text { get; init; } = default!;

        public string? AttachmentUrl { get; set; }

        public string? AttachmentFileName { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<Comment> ChildComments { get; init; } = [];
    }
}