using Discussly.Server.DTO.Users;
using MediatR;

namespace Discussly.Server.Services.Queries.Users
{
    public class GetUserInfoQuery : IRequest<UserInfo>;
}