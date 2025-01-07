using AutoMapper;
using Discussly.Server.DTO;
using Discussly.Server.Endpoints.Comments;
using Discussly.Server.SharedKernel.DTO;

namespace Discussly.Server.Mapping
{
    public class CommentMapperProfile : Profile
    {
        public CommentMapperProfile()
        {
            CreateMap<AddCommentRequest, CommentDto>().ReverseMap();
            CreateMap<Data.Entities.Comments.Comment, Comment>().ReverseMap();

            CreateMap<CommentDto, Comment>()
                .ForMember(dest => dest.ChildComments, opt => opt.MapFrom(src => src.ChildComments))
                .ReverseMap();
        }
    }
}