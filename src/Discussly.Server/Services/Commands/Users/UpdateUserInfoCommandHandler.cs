using Discussly.Server.Endpoints.Users;
using Discussly.Server.Exceptions;
using Discussly.Server.Services.Interfaces;
using Discussly.Server.SharedKernel.DTO;
using MassTransit;
using MediatR;
using System.Security.Claims;

namespace Discussly.Server.Services.Commands.Users
{
    public class UpdateUserInfoCommandHandler(IHttpContextAccessor httpContextAccessor, IChunkedFileSenderService chunkedFileSenderService, IBus bus)
        : IRequestHandler<UpdateUserInfoCommand>
    {
        public async Task Handle(UpdateUserInfoCommand request, CancellationToken cancellationToken)
        {
            var claims = httpContextAccessor.HttpContext!.User.Claims;
            var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                ?? throw new ForbiddenException("User not found");

            var userSettingsDto = MapperService.Map<UpdateUserInfoRequest, UserInfoDto>(request.Model);
            userSettingsDto.UserId = userId;

            if (request.Model.AvatarAction == "Update")
            {
                if (request.Model.FileStream != null && request.Model.FileStream.Length > 0)
                {
                    var fileExtension = System.IO.Path.GetExtension(request.Model.FileStream.FileName).ToLower();
                    var fileName = request.Model.FileStream.FileName;

                    if (!IsAllowedFileType(fileExtension))
                    {
                        throw new ForbiddenException("Only JPG, and PNG files are allowed.");
                    }

                    var memoryStream = new MemoryStream();
                    await request.Model.FileStream.CopyToAsync(memoryStream, cancellationToken);
                    memoryStream.Position = 0;

                    Stream processedStream = memoryStream;

                    if (IsImageFile(fileExtension))
                    {
                        var imageResizer = new ImageResizer();
                        processedStream = await imageResizer.Resize(memoryStream, fileExtension, 256, 256);
                    }

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await chunkedFileSenderService.SendAvatarFileAsync(
                                processedStream,
                                userId,
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
            }
            else if (request.Model.AvatarAction == "Delete")
            {
                var deleteUserAvatarDto = new DeleteUserAvatarDto { Id = userId };
                await bus.Publish(deleteUserAvatarDto, cancellationToken);
            }

            await bus.Publish(userSettingsDto, cancellationToken);
        }

        private static bool IsAllowedFileType(string fileExtension) => fileExtension switch
        {
            ".jpg" or ".jpeg" or ".png" => true,
            _ => false
        };

        private static bool IsImageFile(string fileExtension) => fileExtension switch
        {
            ".jpg" or ".jpeg" or ".png" => true,
            _ => false
        };
    }
}