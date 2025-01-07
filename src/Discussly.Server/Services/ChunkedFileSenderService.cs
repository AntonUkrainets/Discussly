using Discussly.Server.Services.Interfaces;
using Discussly.Server.SharedKernel.DTO;
using MassTransit;

namespace Discussly.Server.Services
{
    public class ChunkedFileSenderService(IBus bus) : IChunkedFileSenderService
    {
        private const int ChunkSize = 256 * 1024;

        public async Task SendCommentFileAsync(Stream fileStream, Guid commentId, string fileExtension, string fileName, CancellationToken cancellationToken)
        {
            var fileId = Guid.NewGuid();
            var chunkIndex = 0;
            var buffer = new byte[ChunkSize];
            int bytesRead;

            while ((bytesRead = await fileStream.ReadAsync(buffer.AsMemory(0, ChunkSize), cancellationToken)) > 0)
            {
                var chunkData = new byte[bytesRead];
                Array.Copy(buffer, chunkData, bytesRead);

                var chunkMessage = new ChunkCommentMessageDto
                {
                    FileId = fileId,
                    ChunkIndex = chunkIndex,
                    IsLastChunk = bytesRead < ChunkSize,
                    ChunkData = chunkData,
                    CommentId = commentId,
                    FileExtension = fileExtension,
                    FileName = fileName
                };

                await bus.Publish(chunkMessage, cancellationToken);

                chunkIndex++;
            }
        }

        public async Task SendAvatarFileAsync(Stream fileStream, string userId, string fileExtension, string fileName, CancellationToken cancellationToken)
        {
            var fileId = Guid.NewGuid();
            var chunkIndex = 0;
            var buffer = new byte[ChunkSize];
            int bytesRead;

            while ((bytesRead = await fileStream.ReadAsync(buffer.AsMemory(0, ChunkSize), cancellationToken)) > 0)
            {
                var chunkData = new byte[bytesRead];
                Array.Copy(buffer, chunkData, bytesRead);

                var chunkMessage = new ChunkAvatarMessageDto
                {
                    FileId = fileId,
                    ChunkIndex = chunkIndex,
                    IsLastChunk = bytesRead < ChunkSize,
                    ChunkData = chunkData,
                    UserId = userId,
                    FileExtension = fileExtension,
                    FileName = fileName
                };

                await bus.Publish(chunkMessage, cancellationToken);

                chunkIndex++;
            }
        }
    }
}