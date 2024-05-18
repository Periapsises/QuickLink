---
layout: _landing
---

# Getting Started

Before getting started with Quick Link, it is important to understand the relation clients have with a server.  
Clients connect to and communicate only with the server. Unlike a [Peer to peer](https://en.wikipedia.org/wiki/Peer-to-peer) connection, clients cannot communicate between them.  
However, the server can send messages to all connected clients allowing for message relaying, if required.

## Host

A host is similar to a client. It can send and receive message to a server.  
The only difference being that the host also creates a local server hosted on their machine.

## Messages

Messages are a stream of data to exchange between the server and clients.
It's a sequence of data types encoded and written to a stream in binary form.  
The receiver can then read those data types in order and process the data how it needs.

Messages are attributed a type which the developer creates depending on the kind of message they need to transfer.
When a message is created with a type, only event listeners attached to that type of message will be called once the message arrives.

---

## Creating a local server

To create a local server, a [Host](<xref:QuickLink.Host>) is needed along with a port on which the server will be listening.

```csharp
using QuickLink;

public class Program
{
    public static void Main()
    {
        int port = 54080;

        using (Host host = new Host(port))
        {
            Console.WriteLine($"Local server created: Listening on port {port}.");
            Console.WriteLine("Press ENTER to stop")
            Console.ReadLine();
        }
    }
}
```

## Creating a client and connecting

When creating a [Client](<xref:QuickLink.Client>), the server's address and port are required.  
Upon connecting or disconnecting from the server, specific events will be fired.

```csharp
using QuickLink;

public class Program
{
    public static void Main()
    {
        string address = "127.0.0.1";
        int port = 54080;

        using (Client client = new Client())
        {
            client.Connected.Subscribe(() => {
                Console.WriteLine("Connected to server.");
            });

            client.Disconnected.Subscribe(() => {
                Console.WriteLine("Disconnected from server.");
            });

            client.Connect(address, port);

            Console.WriteLine("Press ENTER to stop")
            Console.ReadLine();
        }
    }
}
```

---

## Creating messages

Creating and writing data to a message can be done using the [MessageWriter](<xref:QuickLink.Messaging.MessageWriter>).  
A [MessageType](<xref:QuickLink.Messaging.MessageType>) needs to be provided to initialize the message.

```csharp
MessageType GenericMessage = MessageType.Get("GenericMessage");

using (MessageWriter writer = new MessageWriter(GenericMessage))
{
    writer.WriteString("Hello, world!");
    writer.WriteFloat(3.141592f);
}
```

These messages can then be sent to the server or to clients using the appropriate methods:

# [Client to Server](#tab/clienttoserver)

```csharp
server.BroadcastMessage(writer);
```

# [Server to Client](#tab/servertoclient)

```csharp
client.SendToServer(writer);
host.SendToServer(writer); // The host is also a client!
```

---

## Receiving messages

Once a message is sent, it will cause the message received events to be fired on the receiver(s).  
Listeners need to be attached to a specific event type and accept a [MessageReader](<xref:QuickLink.Messaging.MessageReader>) parameter.

```csharp
MessageType GenericMessage = MessageType.Get("GenericMessage");

client.MessageReceived.Subscribe(GenericMessage, (reader) = {
    string message = reader.ReadString();
    float pi = reader.ReadFloat();

    Console.WriteLine($"Received message '{message}' and PI is: {pi}!");
});
```

> [!NOTE]
> The handler field name is the same (`MessageReceived`) for clients, host and server.