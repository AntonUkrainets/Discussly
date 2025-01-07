namespace Discussly.Server.DTO.Responses
{
    public class CaptchaResponse
    {
        public MemoryStream Image { get; set; } = default!;
    }
}