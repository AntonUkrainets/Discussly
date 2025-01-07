using Discussly.Server.Data.Domain.Interfaces;
using Discussly.Server.Data.Entities.Comments;
using Discussly.Server.Infrastructure.Services.Elastic.Interfaces;
using Discussly.Server.Infrastructure.Services.WebSockets;
using Discussly.Server.SharedKernel.DTO;
using MassTransit;
using System.Text.RegularExpressions;

namespace Discussly.Server.Infrastructure.Consumers
{
    public class CommentConsumer(
        IDiscussionDataUnitOfWork discussionDataUnitOfWork,
        IElasticSearchService elasticSearchService,
        WebSocketHandler webSocketHandler
    ) : IConsumer<CommentDto>
    {
        public async Task Consume(ConsumeContext<CommentDto> context)
        {
            var commentDto = context.Message;

            var rootCommentId = commentDto.RootCommentId ?? commentDto.Id;

            var parentCommentId = commentDto.ParentCommentId.HasValue
                ? commentDto.ParentCommentId
                : null;

            var utcNow = DateTime.UtcNow;
            var sanitisedText = SanitiseMessage(commentDto.Text);

            var comment = new Comment
            {
                Id = commentDto.Id,
                RootCommentId = rootCommentId,
                ParentCommentId = parentCommentId,
                UserId = commentDto.UserId,
                Text = sanitisedText,
                CreatedAt = utcNow
            };

            await discussionDataUnitOfWork.Comments.AddCommentAsync(comment);
            await discussionDataUnitOfWork.SaveAsync();

            if (parentCommentId == null)
            {
                comment.User = await discussionDataUnitOfWork.Users.GetUserByIdAsync(commentDto.UserId);
                await elasticSearchService.AddCommentAsync(comment);
            }

            if (!commentDto.HasFile)
            {
                await webSocketHandler.BroadcastNewComment(comment.Id);
            }
        }

        private static string SanitiseMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return string.Empty;

            message = message
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\\n", "\n");

            message = Regex.Replace(message,
                @"\[a\s+href=""([^""]+)""(?:\s+title=""([^""]*)"")?\](.*?)\[/a\]",
                match =>
                {
                    string href = match.Groups[1].Value;
                    string title = match.Groups[2].Success ? $" title=\"{match.Groups[2].Value}\"" : "";
                    string content = match.Groups[3].Value;
                    return $"<a href=\"{href}\"{title}>{content}</a>";
                },
                RegexOptions.Singleline);

            message = message
                .Replace("[i]", "<i>")
                .Replace("[/i]", "</i>")
                .Replace("[code]", "<code>")
                .Replace("[/code]", "</code>")
                .Replace("[strong]", "<strong>")
                .Replace("[/strong]", "</strong>");

            return message;
        }
    }
}