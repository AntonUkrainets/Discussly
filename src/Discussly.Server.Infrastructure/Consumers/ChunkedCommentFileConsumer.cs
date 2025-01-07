﻿using Discussly.Server.Data.Domain.Interfaces;
using Discussly.Server.Data.Entities.Comments;
using Discussly.Server.Infrastructure.Services.Azure.Interfaces;
using Discussly.Server.Infrastructure.Services.WebSockets;
using Discussly.Server.SharedKernel.DTO;
using MassTransit;
using System.Collections.Concurrent;

namespace Discussly.Server.Infrastructure.Consumers
{
    public class ChunkedCommentFileConsumer(
        IBlobStorageService blobStorageService,
        IDiscussionDataUnitOfWork discussionDataUnitOfWork,
        WebSocketHandler webSocketHandler
    )
        : IConsumer<ChunkCommentMessageDto>
    {
        private static readonly ConcurrentDictionary<Guid, SortedDictionary<int, byte[]>> FileChunks = new();

        public async Task Consume(ConsumeContext<ChunkCommentMessageDto> context)
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

                var updateCommentAttachment = new UpdateCommentAttachment
                {
                    CommentId = message.CommentId,
                    AttachmentFileName = message.FileName,
                    AttachmentUrl = blob.Url
                };

                await discussionDataUnitOfWork.Comments.UpdateCommentAttachmentAsync(updateCommentAttachment);
                await discussionDataUnitOfWork.SaveAsync();

                FileChunks.TryRemove(message.FileId, out _);

                await webSocketHandler.BroadcastNewComment(message.CommentId);
            }
        }

        private static async Task UploadFileToAzureAsync(Stream stream, SortedDictionary<int, byte[]> chunks)
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