using Discussly.Server.SharedKernel.DTO;
using Microsoft.AspNetCore.Mvc;

namespace Discussly.Server.DTO.Requests
{
    public class GetCommentsRequest
    {
        [FromBody]
        public int PageIndex { get; set; }

        [FromBody]
        public int PageSize { get; set; }

        public string? SearchText { get; set; }

        public CommentSortByDto? SortBy { get; set; }

        public CommentSortDirectionDto? SortDirection { get; set; }
    }
}