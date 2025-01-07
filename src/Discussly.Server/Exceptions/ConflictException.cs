namespace Discussly.Server.Exceptions
{
    public class ConflictException(string? message = default) : Exception(message);
}