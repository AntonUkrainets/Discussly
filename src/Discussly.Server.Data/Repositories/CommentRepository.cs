using Discussly.Server.Data.Domain;
using Discussly.Server.Data.Entities.Comments;
using Discussly.Server.Data.Repositories.Interfaces;
using Discussly.Server.SharedKernel.DTO;
using Microsoft.EntityFrameworkCore;

namespace Discussly.Server.Data.Repositories
{
    public class CommentRepository(DiscussionDataContext discussionDataContext) : ICommentRepository
    {
        public async Task AddCommentAsync(Comment comment, CancellationToken cancellationToken)
        {
            await discussionDataContext.Comments.AddAsync(comment, cancellationToken);
        }

        public async Task<CommentDto[]> GetLatestCommentsAsync(Guid[] rootCommentsIds, CancellationToken cancellationToken)
        {
            var comments = await LoadCommentsAsync(rootCommentsIds, cancellationToken);

            return comments;
        }

        private async Task<CommentDto[]> LoadCommentsAsync(Guid[] rootCommentIds, CancellationToken cancellationToken)
        {
            var allComments = await discussionDataContext.Comments
                .Include(x => x.User)
                .Where(c => rootCommentIds.Contains(c.RootCommentId))
                .Select(x =>
                    new CommentDto
                    {
                        Id = x.Id,
                        UserName = x.User.UserName!,
                        UserAvatar = x.User.AvatarUrl,
                        RootCommentId = x.RootCommentId,
                        ParentCommentId = x.ParentCommentId,
                        Text = x.Text,
                        AttachmentFileName = x.AttachmentFileName,
                        AttachmentUrl = x.AttachmentUrl,
                        CreatedAt = x.CreatedAt
                    })
                .ToArrayAsync(cancellationToken);

            foreach (var comment in allComments)
            {
                comment.ChildComments = allComments
                    .Where(x => x.ParentCommentId == comment.Id)
                    .ToArray();
            }

            var rootComments = allComments.Where(x => x.ParentCommentId == null);

            var rootCommentsOrdered = rootComments
                .OrderBy(rc => Array.IndexOf(rootCommentIds, rc.Id))
                .ToArray();

            return rootCommentsOrdered;
        }

        public async Task<bool> IsCommentExistsAsync(Guid id, CancellationToken cancellationToken)
        {
            var comment = await discussionDataContext.Comments.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            return comment != null;
        }

        public async Task<Comment[]> GetAllCommentsAsync(CancellationToken cancellationToken = default)
        {
            var rootComments = await discussionDataContext.Comments
               .Where(c => c.ParentCommentId == null)
               .Include(i => i.User)
               .OrderByDescending(c => c.CreatedAt)
               .Select(c => new Comment
               {
                   Id = c.Id,
                   UserId = c.UserId,
                   Text = c.Text,
                   CreatedAt = c.CreatedAt,
                   ParentCommentId = c.ParentCommentId,
                   RootCommentId = c.RootCommentId,
                   AttachmentFileName = c.AttachmentFileName,
                   AttachmentUrl = c.AttachmentUrl,
                   User = c.User
               })
               .ToArrayAsync(cancellationToken);

            var comments = new List<CommentDto>();

            foreach (var rootComment in rootComments)
            {
                var comment = await LoadCommentAsync(rootComment.Id, cancellationToken);

                comments.Add(comment);
            }

            return rootComments;
        }

        private async Task<CommentDto> LoadCommentAsync(Guid rootCommentId, CancellationToken cancellationToken)
        {
            var allComments = await discussionDataContext.Comments
                .Include(x => x.User)
                .Where(c => c.RootCommentId == rootCommentId)
                .Select(x =>
                    new CommentDto
                    {
                        Id = x.Id,
                        UserName = x.User.UserName!,
                        RootCommentId = x.RootCommentId,
                        ParentCommentId = x.ParentCommentId,
                        Text = x.Text,
                        CreatedAt = x.CreatedAt
                    })
                .ToArrayAsync(cancellationToken);

            foreach (var comment in allComments)
            {
                comment.ChildComments = allComments
                    .Where(x => x.ParentCommentId == comment.Id)
                    .ToArray();
            }

            var rootComment = allComments.First(x => x.ParentCommentId == null);

            return rootComment;
        }

        public async Task UpdateCommentAttachmentAsync(UpdateCommentAttachment comment, CancellationToken cancellationToken = default)
        {
            await discussionDataContext.Comments
                .Where(c => c.Id == comment.CommentId)
                .ExecuteUpdateAsync(st => st
                    .SetProperty(p => p.AttachmentFileName, comment.AttachmentFileName)
                    .SetProperty(p => p.AttachmentUrl, comment.AttachmentUrl),
                    cancellationToken);
        }

        public async Task<CommentDto> GetCommentByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await discussionDataContext.Comments
                .Where(c => c.Id == id)
                .Select(c =>
                    new CommentDto
                    {
                        Id = c.Id,
                        UserName = c.User.UserName!,
                        UserAvatar = c.User.AvatarUrl,
                        RootCommentId = c.RootCommentId,
                        ParentCommentId = c.ParentCommentId,
                        Text = c.Text,
                        AttachmentFileName = c.AttachmentFileName,
                        AttachmentUrl = c.AttachmentUrl,
                        CreatedAt = c.CreatedAt
                    })
                .FirstAsync(cancellationToken);
        }
    }
}