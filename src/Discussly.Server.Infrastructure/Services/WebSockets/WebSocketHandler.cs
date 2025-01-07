using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace Discussly.Server.Infrastructure.Services.WebSockets
{
    public class WebSocketHandler
    {
        private static readonly ConcurrentDictionary<WebSocket, int> _sockets = new();

        public async Task HandleConnection(WebSocket webSocket)
        {
            _sockets.TryAdd(webSocket, 0);
            var buffer = new byte[1024 * 4];

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", CancellationToken.None);
                    }
                }
            }
            catch (WebSocketException ex)
            {
                Console.WriteLine($"WebSocket Exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected Exception: {ex.Message}");
            }
            finally
            {
                if (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseReceived)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing connection", CancellationToken.None);
                }
                _sockets.TryRemove(webSocket, out _);
            }
        }

        public async Task BroadcastNewComment(Guid commentId)
        {
            var message = new
            {
                type = "NEW_COMMENT",
                comment_id = commentId
            };

            var messageJson = JsonConvert.SerializeObject(message);
            var messageData = Encoding.UTF8.GetBytes(messageJson);

            foreach (var socket in _sockets.Keys)
            {
                if (socket.State == WebSocketState.Open)
                {
                    try
                    {
                        await socket.SendAsync(messageData, WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    catch (WebSocketException ex)
                    {
                        Console.WriteLine($"Failed to send message: {ex.Message}");
                        _sockets.TryRemove(socket, out _);
                    }
                }
                else
                {
                    _sockets.TryRemove(socket, out _);
                }
            }
        }
    }
}