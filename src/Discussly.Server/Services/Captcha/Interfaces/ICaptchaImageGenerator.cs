namespace Discussly.Server.Services.Captcha.Interfaces
{
    public interface ICaptchaImageGenerator
    {
        MemoryStream Generate(string captchaCode);
    }
}
