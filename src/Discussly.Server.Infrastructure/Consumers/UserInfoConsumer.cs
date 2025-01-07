using Discussly.Server.Data.Domain.Interfaces;
using Discussly.Server.Data.Entities.Users;
using Discussly.Server.SharedKernel.DTO;
using MassTransit;

namespace Discussly.Server.Infrastructure.Consumers
{
    public class UserInfoConsumer(IDiscussionDataUnitOfWork discussionDataUnitOfWork) : IConsumer<UserInfoDto>
    {
        public async Task Consume(ConsumeContext<UserInfoDto> context)
        {
            var userSettingsDto = context.Message;

            var user = new User
            {
                Id = userSettingsDto.UserId,
                HomePage = userSettingsDto.HomePage
            };

            await discussionDataUnitOfWork.Users.UpdateUserInfoAsync(user);
            await discussionDataUnitOfWork.SaveAsync();
        }
    }
}