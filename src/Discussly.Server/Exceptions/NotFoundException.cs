namespace Discussly.Server.Exceptions
{
    public class NotFoundException(string? message = default) : Exception(message);
}