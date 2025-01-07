using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Discussly.Server.Infrastructure.Services.Azure.Interfaces;
using Discussly.Server.SharedKernel.Settings.Azure;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;

namespace Discussly.Server.Infrastructure.Services.Azure
{
    public class BlobStorageService(IOptions<AzureBlobStorageSettings> options) : IBlobStorageService
    {
        private readonly BlobContainerClient _containerClient = new(options.Value.ConnectionString, options.Value.ContainerName);

        public async Task<UploadBlob> CreateUploadBlobAsync(string fileName, CancellationToken cancellationToken)
        {
            var blobId = Guid.NewGuid();
            var fileExtension = Path.GetExtension(fileName);
            var blobName = $"{blobId}{fileExtension}";

            var blobClient = _containerClient.GetBlobClient(blobName);
            var uploadBlob = new UploadBlob { Url = blobClient.Uri.AbsoluteUri };

            var blobOpenWriteOptions = new BlobOpenWriteOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = GetContentType(fileExtension)
                }
            };

            try
            {
                uploadBlob.Stream = await blobClient.OpenWriteAsync(true, blobOpenWriteOptions, cancellationToken: cancellationToken);
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == "ContainerNotFound")
            {
                await _containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);

                uploadBlob.Stream = await blobClient.OpenWriteAsync(true, blobOpenWriteOptions, cancellationToken: cancellationToken);
            }

            return uploadBlob;
        }

        private static string GetContentType(string fileExtension)
        {
            var provider = new FileExtensionContentTypeProvider();

            if (provider.TryGetContentType(fileExtension, out var contentType))
                return contentType;

            return "application/octet-stream";
        }

        public async Task DeleteBlobAsync(string blobName, CancellationToken cancellationToken = default)
        {
            var blobClient = _containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
        }
    }
}