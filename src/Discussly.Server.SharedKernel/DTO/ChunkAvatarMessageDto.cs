namespace Discussly.Server.SharedKernel.DTO
{
    public class ChunkAvatarMessageDto
    {
        public string UserId { get; set; } = default!;

        public Guid FileId { get; set; }

        public int ChunkIndex { get; set; }

        public bool IsLastChunk { get; set; }

        public byte[] ChunkData { get; set; } = [];

        public string FileExtension { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;
    }
}