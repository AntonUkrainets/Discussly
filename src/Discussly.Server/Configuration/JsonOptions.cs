using System.Text.Json;

namespace Discussly.Server.Configuration
{
    public static class JsonOptions
    {
        public static JsonSerializerOptions DefaultJsonSerializerOptions { get; }
            = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }
}