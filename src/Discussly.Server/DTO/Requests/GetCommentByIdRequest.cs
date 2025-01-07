using Microsoft.AspNetCore.Mvc;

namespace Discussly.Server.DTO.Requests
{
    public class GetCommentByIdRequest
    {
        [FromBody]
        public Guid Id { get; set; }
    }
}