using Discussly.Server.Data.Domain;
using Discussly.Server.Extensions;
using Discussly.Server.Infrastructure.Services.WebSockets;
using Discussly.Server.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddCorsPolicy()
    .AddSqlConnection<DiscussionDataContext>(builder.Configuration)
    .AddCaching()
    .AddRedis(builder.Configuration)
    .AddServices(builder.Configuration)
    .AddSessions()
    .AddIdentityServer4(builder.Configuration)
    .AddElasticSearch(builder.Configuration)
    .AddRabbitMQ(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<DiscussionDataContext>();
    context.Database.EnsureCreated();
}

app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseWebSockets();

app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var webSocketHandler = context.RequestServices.GetRequiredService<WebSocketHandler>();
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        await webSocketHandler.HandleConnection(webSocket);
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

app.UseResponseCompression();
app.UseCors("MyCorsPolicy");
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapGraphQL();
app.MapGraphQLSchema();
app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();