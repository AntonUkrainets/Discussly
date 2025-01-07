namespace Discussly.Server.Services.Interfaces
{
    public interface IImageHandlerService
    {
        Task UploadFileAsync(
            Stream fileStream,
            string fileName,
            string fileExtension,
            Guid commentId,
            CancellationToken cancellationToken
        );
    }
}