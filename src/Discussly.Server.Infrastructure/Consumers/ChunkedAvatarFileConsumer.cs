using Discussly.Server.Data.Domain.Interfaces;
using Discussly.Server.Infrastructure.Services.Azure.Interfaces;
using Discussly.Server.SharedKernel.DTO;
using MassTransit;
using System.Collections.Concurrent;

namespace Discussly.Server.Infrastructure.Consumers
{
    public class ChunkedAvatarFileConsumer(IBlobStorageService blobStorageService, IDiscussionDataUnitOfWork discussionDataUnitOfWork)
        : IConsumer<ChunkAvatarMessageDto>
    {
        private static readonly ConcurrentDictionary<Guid, SortedDictionary<int, byte[]>> FileChunks = new();

        public async Task Consume(ConsumeContext<ChunkAvatarMessageDto> context)
        {
            var message = context.Message;

            if (!FileChunks.TryGetValue(message.FileId, out SortedDictionary<int, byte[]>? value))
            {
                value = ([]);
                FileChunks[message.FileId] = value;
            }

            value[message.ChunkIndex] = message.ChunkData;

            if (message.IsLastChunk)
            {
                if (!FileChunks.TryGetValue(message.FileId, out var chunks))
                    return;

                var blob = await blobStorageService.CreateUploadBlobAsync(message.FileName);
                if (blob == null)
                    return;

                await UploadFileToAzureAsync(blob.Stream, chunks);

                await discussionDataUnitOfWork.Users.UpdateUserAvatarAsync(message.UserId, blob.Url);
                await discussionDataUnitOfWork.SaveAsync();

                FileChunks.TryRemove(message.FileId, out _);
            }
        }

        private async Task UploadFileToAzureAsync(Stream stream, SortedDictionary<int, byte[]> chunks)
        {
            using (stream)
            {
                foreach (var chunk in chunks.OrderBy(c => c.Key))
                {
                    await stream.WriteAsync(chunk.Value.AsMemory(0, chunk.Value.Length));
                }
            }
        }
    }
}