using Discussly.Server.DTO.Responses;
using Discussly.Server.Endpoints.Captcha;
using MediatR;

namespace Discussly.Server.Services.Queries.Captcha
{
    public class GetCaptchaQuery(GetCaptchaRequest request) : IRequest<CaptchaResponse>
    {
        public GetCaptchaRequest Model { get; } = request;
    }
}