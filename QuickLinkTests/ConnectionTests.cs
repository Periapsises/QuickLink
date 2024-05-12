using QuickLink;

namespace QuickLinkTests;

public class ConnectionTests
{
    [Fact(DisplayName = "A host and a client are able to connect to each other.")]
    public async Task HostAndClientCanConnect()
    {
        using (var host = new Host(50000))
        using (var client = new Client())
        {
            await client.Connect("localhost", 50000);
        }
    }

    [Fact(DisplayName = "The server fires the proper events when a client connects or disconnects.")]
    public async Task ServerFiresEventsOnClientConnectAndDisconnect()
    {
        TimeSpan timeout = TimeSpan.FromSeconds(5);

        TaskCompletionSource<bool> clientConnected = new();
        TaskCompletionSource<bool> clientDisconnected = new();

        using (var host = new Host(50001))
        {
            host.Server.ClientConnected.Subscribe((client) => { Assert.True(clientConnected.TrySetResult(true)); });
            host.Server.ClientDisconnected.Subscribe((client) => { Assert.True(clientDisconnected.TrySetResult(true)); });

            using (var client = new Client())
            {
                await client.Connect("localhost", 50001);

                await Task.WhenAny(clientConnected.Task, Task.Delay(timeout));
                Assert.True(clientConnected.Task.IsCompleted);
            }

            await Task.WhenAny(clientDisconnected.Task, Task.Delay(timeout));
            Assert.True(clientDisconnected.Task.IsCompleted);
        }
    }

    [Fact(DisplayName = "The client fires the proper events when it connects or disconnects.")]
    public async Task ClientFiresEventsOnConnectAndDisconnect()
    {
        TimeSpan timeout = TimeSpan.FromSeconds(5);

        TaskCompletionSource<bool> connected = new();
        TaskCompletionSource<bool> disconnected = new();

        using (var host = new Host(50002))
        {
            using (var client = new Client())
            {
                client.Connected.Subscribe(() => { Assert.True(connected.TrySetResult(true)); });
                client.Disconnected.Subscribe(() => { Assert.True(disconnected.TrySetResult(true)); });

                await client.Connect("localhost", 50002);

                await Task.WhenAny(connected.Task, Task.Delay(timeout));
                Assert.True(connected.Task.IsCompleted);
            }

            await Task.WhenAny(disconnected.Task, Task.Delay(timeout));
            Assert.True(disconnected.Task.IsCompleted);
        }
    }
}