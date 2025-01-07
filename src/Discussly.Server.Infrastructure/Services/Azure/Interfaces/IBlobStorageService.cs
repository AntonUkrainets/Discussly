namespace Discussly.Server.Infrastructure.Services.Azure.Interfaces
{
    public interface IBlobStorageService
    {
        Task<UploadBlob> CreateUploadBlobAsync(string fileName, CancellationToken cancellationToken = default);

        Task DeleteBlobAsync(string blobName, CancellationToken cancellationToken = default);
    }
}