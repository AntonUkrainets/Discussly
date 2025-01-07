using Microsoft.AspNetCore.Mvc;

namespace Discussly.Server.Endpoints.Captcha
{
    public class GetCaptchaRequest
    {
        [FromQuery]
        public string Email { get; set; } = default!;
    }
}