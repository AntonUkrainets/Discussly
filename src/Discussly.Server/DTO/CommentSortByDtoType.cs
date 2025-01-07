using Discussly.Server.SharedKernel.DTO;

namespace Discussly.Server.DTO
{
    public class CommentSortByDtoType : EnumType<CommentSortByDto>
    {
        protected override void Configure(IEnumTypeDescriptor<CommentSortByDto> descriptor)
        {
            descriptor.Value(CommentSortByDto.UserName).Name(nameof(CommentSortByDto.UserName));
            descriptor.Value(CommentSortByDto.Email).Name(nameof(CommentSortByDto.Email));
            descriptor.Value(CommentSortByDto.CreatedDate).Name(nameof(CommentSortByDto.CreatedDate));
        }
    }
}