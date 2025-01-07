using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace Discussly.Server.Services
{
    public class ImageResizer
    {
        public async Task<Stream> Resize(Stream originalStream, string fileExtension, int maxWidth, int maxHeight)
        {
            originalStream.Position = 0;
            using var image = await Image.LoadAsync(originalStream);
            if (image.Width <= maxWidth && image.Height <= maxHeight)
            {
                originalStream.Position = 0;
                return originalStream;
            }

            image.Mutate(x => x.Resize(maxWidth, maxHeight));

            var resizedStream = new MemoryStream();
            await image.SaveAsync(resizedStream, GetImageFormat(fileExtension));
            resizedStream.Position = 0;

            return resizedStream;
        }

        private static IImageEncoder GetImageFormat(string fileExtension) => fileExtension switch
        {
            ".jpg" or ".jpeg" => new JpegEncoder(),
            ".png" => new PngEncoder(),
            _ => new JpegEncoder()
        };
    }
}