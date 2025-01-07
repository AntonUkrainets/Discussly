using Discussly.Server.SharedKernel.DTO;

namespace Discussly.Server.DTO
{
    public class CommentSortDirectionDtoType : EnumType<CommentSortDirectionDto>
    {
        protected override void Configure(IEnumTypeDescriptor<CommentSortDirectionDto> descriptor)
        {
            descriptor.Value(CommentSortDirectionDto.ASC).Name(nameof(CommentSortDirectionDto.ASC));
            descriptor.Value(CommentSortDirectionDto.DESC).Name(nameof(CommentSortDirectionDto.DESC));
        }
    }
}