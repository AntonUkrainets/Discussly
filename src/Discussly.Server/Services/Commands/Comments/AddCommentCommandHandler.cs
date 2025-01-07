using Discussly.Server.Data.Domain.Interfaces;
using Discussly.Server.Endpoints.Comments;
using Discussly.Server.Exceptions;
using Discussly.Server.Services.Interfaces;
using Discussly.Server.SharedKernel.DTO;
using MassTransit;
using MediatR;
using System.Security.Claims;

namespace Discussly.Server.Services.Commands.Comments
{
    public class AddCommentCommandHandler(
        IHttpContextAccessor httpContextAccessor,
        IDiscussionDataUnitOfWork discussionDataUnitOfWork,
        IChunkedFileSenderService chunkedFileSenderService,
        IBus bus
    )
        : IRequestHandler<AddCommentCommand>
    {
        public async Task Handle(AddCommentCommand request, CancellationToken cancellationToken)
        {
            var claims = httpContextAccessor.HttpContext!.User.Claims;
            var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                ?? throw new ForbiddenException("User not found");

            if (!await discussionDataUnitOfWork.Users.IsUserExistsByIdAsync(userId, cancellationToken))
                throw new ForbiddenException("User not found");

            if (request.Model.ParentCommentId.HasValue)
            {
                if (!await discussionDataUnitOfWork.Comments.IsCommentExistsAsync(request.Model.ParentCommentId.Value, cancellationToken))
                    throw new ForbiddenException("Parent does not exists");
            }

            var comment = MapperService.Map<AddCommentRequest, CommentDto>(request.Model);
            comment.UserId = userId;
            comment.Id = Guid.NewGuid();

            var hasFile = request.Model.FileStream != null && request.Model.FileStream.Length > 0;
            if (hasFile)
            {
                var fileExtension = System.IO.Path.GetExtension(request.Model.FileStream!.FileName).ToLower();
                var fileName = request.Model.FileStream.FileName;

                if (!IsAllowedFileType(fileExtension))
                    throw new ForbiddenException("Only JPG, GIF, PNG, and TXT files are allowed.");

                var memoryStream = new MemoryStream();
                await request.Model.FileStream.CopyToAsync(memoryStream, cancellationToken);
                memoryStream.Position = 0;

                Stream processedStream = memoryStream;

                if (IsImageFile(fileExtension))
                {
                    var imageResizer = new ImageResizer();
                    processedStream = await imageResizer.Resize(memoryStream, fileExtension, 320, 240);
                }

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await chunkedFileSenderService.SendCommentFileAsync(
                            processedStream,
                            comment.Id,
                            fileExtension,
                            fileName,
                            cancellationToken
                        );
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error uploading file: {ex.Message}");
                    }
                    finally
                    {
                        await processedStream.DisposeAsync();
                        await memoryStream.DisposeAsync();
                    }
                }, cancellationToken);
            }

            comment.HasFile = hasFile;
            await bus.Publish(comment, cancellationToken);
        }

        private static bool IsAllowedFileType(string fileExtension) => fileExtension switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".txt" => true,
            _ => false
        };

        private static bool IsImageFile(string fileExtension) => fileExtension switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" => true,
            _ => false
        };
    }
}