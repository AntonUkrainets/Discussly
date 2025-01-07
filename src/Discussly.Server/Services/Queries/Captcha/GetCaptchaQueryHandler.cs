using Discussly.Server.Data.Domain.Interfaces;
using Discussly.Server.DTO.Responses;
using Discussly.Server.Services.Captcha.Interfaces;
using MediatR;

namespace Discussly.Server.Services.Queries.Captcha
{
    public class GetCaptchaQueryHandler(
        ICaptchaImageGenerator imageGenerator,
        IDiscussionDataUnitOfWork discussionDataUnitOfWork
    ) : IRequestHandler<GetCaptchaQuery, CaptchaResponse>
    {
        public async Task<CaptchaResponse> Handle(GetCaptchaQuery request, CancellationToken cancellationToken)
        {
            var captcha = await GenerateAsync(request.Model.Email, cancellationToken);

            return new CaptchaResponse { Image = captcha };
        }

        private async Task<MemoryStream> GenerateAsync(string userEmail, CancellationToken cancellationToken)
        {
            var captchaCode = $"{new Random().Next(100000, 999999)}";

            await discussionDataUnitOfWork.Sessions.AddOrUpdateSessionAsync(userEmail, captchaCode, cancellationToken);

            var captchaImage = imageGenerator.Generate(captchaCode);

            return captchaImage;
        }
    }
}