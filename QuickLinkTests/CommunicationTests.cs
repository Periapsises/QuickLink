using QuickLink;

namespace QuickLinkTests;

public class CommunicationTests
{
    public MessageType MessageType1 = MessageType.Get("MessageType1");

    [Fact(DisplayName = "A client can send a message to the server.", Timeout = 10000)]
    public async Task ClientCanSendMessageToServer()
    {
        using (Host host = new Host(51000))
        using (Client client = new Client())
        {
            await client.Connect("localhost", 51000);

            TaskCompletionSource<bool> taskCompletion = new TaskCompletionSource<bool>();

            host.Server.MessageReceived.Subscribe(MessageType1, (message) => {
                Assert.Equal(1, message.ReadInt16());
                Assert.Equal(2, message.ReadInt32());
                Assert.Equal("Hi", message.ReadString());

                taskCompletion.SetResult(true);
            });

            using (MessageWriter message = new MessageWriter(MessageType1))
            {
                message.WriteInt16(1);
                message.WriteInt32(2);
                message.WriteString("Hi");

                client.SendToServer(message);
            }

            await taskCompletion.Task;
        }
    }

    [Fact(DisplayName = "All clients connected to the server receive broadcasted messages.", Timeout = 10000)]
    public async Task ServerCanSendMessageToClient()
    {
        using (Host host = new Host(51001))
        using (Client client1 = new Client())
        using (Client client2 = new Client())
        {
            await client1.Connect("localhost", 51001);
            await client2.Connect("localhost", 51001);

            Assert.Equal(ConnectionState.Connected, client1.ConnectionState);
            Assert.Equal(ConnectionState.Connected, client2.ConnectionState);

            TaskCompletionSource<bool> taskCompletion1 = new TaskCompletionSource<bool>();
            TaskCompletionSource<bool> taskCompletion2 = new TaskCompletionSource<bool>();

            client1.MessageReceived.Subscribe(MessageType1, (message) => {
                Assert.Equal(1, message.ReadInt16());
                Assert.Equal(2, message.ReadInt32());
                Assert.Equal("Hi", message.ReadString());

                taskCompletion1.SetResult(true);
            });

            client2.MessageReceived.Subscribe(MessageType1, (message) => {
                Assert.Equal(1, message.ReadInt16());
                Assert.Equal(2, message.ReadInt32());
                Assert.Equal("Hi", message.ReadString());

                taskCompletion2.SetResult(true);
            });

            using (MessageWriter message = new MessageWriter(MessageType1))
            {
                message.WriteInt16(1);
                message.WriteInt32(2);
                message.WriteString("Hi");

                host.Server.BroadcastMessage(message);
            }

            await taskCompletion1.Task;
            await taskCompletion2.Task;
        }
    }

    [Fact(DisplayName = "The host of the server receives broadcasted messages.", Timeout = 10000)]
    public async Task ServerCanSendMessageToHost()
    {
        using (Host host = new Host(51002))
        {
            TaskCompletionSource<bool> taskCompletion = new TaskCompletionSource<bool>();

            host.MessageReceived.Subscribe(MessageType1, (message) => {
                Assert.Equal(1, message.ReadInt16());
                Assert.Equal(2, message.ReadInt32());
                Assert.Equal("Hi", message.ReadString());

                taskCompletion.SetResult(true);
            });

            using (MessageWriter message = new MessageWriter(MessageType1))
            {
                message.WriteInt16(1);
                message.WriteInt32(2);
                message.WriteString("Hi");

                host.Server.BroadcastMessage(message);
            }

            await taskCompletion.Task;
        }
    }

    [Fact(DisplayName = "The host of the server can send messages to the server.", Timeout = 10000)]
    public async Task HostCanSendMessageToServer()
    {
        using (Host host = new Host(51003))
        {
            TaskCompletionSource<bool> taskCompletion = new TaskCompletionSource<bool>();

            host.Server.MessageReceived.Subscribe(MessageType1, (message) => {
                Assert.Equal(1, message.ReadInt16());
                Assert.Equal(2, message.ReadInt32());
                Assert.Equal("Hi", message.ReadString());

                taskCompletion.SetResult(true);
            });

            using (MessageWriter message = new MessageWriter(MessageType1))
            {
                message.WriteInt16(1);
                message.WriteInt32(2);
                message.WriteString("Hi");

                host.SendToServer(message);
            }

            await taskCompletion.Task;
        }
    }
}