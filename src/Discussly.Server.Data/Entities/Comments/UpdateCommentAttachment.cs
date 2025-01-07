namespace Discussly.Server.Data.Entities.Comments
{
    public class UpdateCommentAttachment
    {
        public Guid CommentId { get; set; }

        public string AttachmentFileName { get; set; } = default!;

        public string AttachmentUrl { get; set; } = default!;
    }
}