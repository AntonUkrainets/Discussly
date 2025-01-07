using Discussly.Server.Services.Captcha.Interfaces;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Discussly.Server.Services.Captcha
{
    public class DefaultCaptchaImageGenerator : ICaptchaImageGenerator
    {
        public MemoryStream Generate(string captchaCode)
        {
            const int width = 200;
            const int height = 60;

            using var image = new Image<Rgba32>(width, height);

            var fontCollection = new FontCollection();
            var fontFamily = fontCollection.Add("/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf");
            var font = fontFamily.CreateFont(24);

            image.Mutate(ctx =>
            {
                ctx.BackgroundColor(Color.LightGray);
                ctx.DrawText(
                    new RichTextOptions(font)
                    {
                        Origin = new PointF(width / 4, height / 4),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        WrappingLength = width
                    },
                    captchaCode,
                    Color.Black
                );
            });

            var memoryStream = new MemoryStream();
            image.SaveAsPng(memoryStream);
            memoryStream.Position = 0;

            return memoryStream;
        }
    }
}