namespace Discussly.Server.Infrastructure.Services.Azure
{
    public class UploadBlob
    {
        public Stream Stream { get; set; } = default!;

        public string Url { get; set; } = default!;
    }
}