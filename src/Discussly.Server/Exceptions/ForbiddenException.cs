﻿namespace Discussly.Server.Exceptions
{
    public class ForbiddenException(string? message) : Exception(message);
}