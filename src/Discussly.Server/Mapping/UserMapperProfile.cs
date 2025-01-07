using AutoMapper;
using Discussly.Server.DTO.Users;
using Discussly.Server.Endpoints.Users;
using Discussly.Server.SharedKernel.DTO;

namespace Discussly.Server.Mapping
{
    public class UserMapperProfile : Profile
    {
        public UserMapperProfile()
        {
            CreateMap<UpdateUserInfoRequest, UserInfoDto>().ReverseMap();
            CreateMap<Data.Entities.Users.UserInfo, UserInfo>().ReverseMap();
        }
    }
}