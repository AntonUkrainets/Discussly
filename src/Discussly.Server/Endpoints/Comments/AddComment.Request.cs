using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Discussly.Server.Endpoints.Comments
{
    public class AddCommentRequest
    {
        [FromForm]
        public Guid? ParentCommentId { get; set; }

        [FromForm]
        public Guid? RootCommentId { get; set; }

        [FromForm]
        [Length(1, 1000)]
        public string Text { get; set; } = default!;

        [FromForm]
        public IFormFile? FileStream { get; set; }
    }
}