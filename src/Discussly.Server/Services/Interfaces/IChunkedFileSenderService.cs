namespace Discussly.Server.Services.Interfaces
{
    public interface IChunkedFileSenderService
    {
        Task SendCommentFileAsync(Stream fileStream, Guid commentId, string fileExtension, string fileName, CancellationToken cancellationToken);

        Task SendAvatarFileAsync(Stream fileStream, string userId, string fileExtension, string fileName, CancellationToken cancellationToken);
    }
}