using Discussly.Server.Data.Domain.Interfaces;
using Discussly.Server.Infrastructure.Services.Azure.Interfaces;
using Discussly.Server.SharedKernel.DTO;
using MassTransit;

namespace Discussly.Server.Infrastructure.Consumers
{
    public class DeleteUserAvatarConsumer(
        IDiscussionDataUnitOfWork discussionDataUnitOfWork,
        IBlobStorageService blobStorageService
        ) : IConsumer<DeleteUserAvatarDto>
    {
        public async Task Consume(ConsumeContext<DeleteUserAvatarDto> context)
        {
            var userSettingsDto = context.Message;

            var user = await discussionDataUnitOfWork.Users.GetUserByIdAsync(userSettingsDto.Id);
            var blobName = ExtractBlobName(user.AvatarUrl);

            await blobStorageService.DeleteBlobAsync(blobName);

            await discussionDataUnitOfWork.Users.UpdateUserAvatarAsync(userSettingsDto.Id, string.Empty);
            await discussionDataUnitOfWork.SaveAsync();
        }

        private static string ExtractBlobName(string? url)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            var uri = new Uri(url);
            return Path.GetFileName(uri.AbsolutePath);
        }
    }
}