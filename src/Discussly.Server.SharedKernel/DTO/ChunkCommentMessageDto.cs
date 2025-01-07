namespace Discussly.Server.SharedKernel.DTO
{
    public class ChunkCommentMessageDto
    {
        public Guid CommentId { get; set; }

        public Guid FileId { get; set; }

        public int ChunkIndex { get; set; }

        public bool IsLastChunk { get; set; }

        public byte[] ChunkData { get; set; } = [];

        public string FileExtension { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;
    }
}