using System;
using System.Threading.Tasks;

using QuickLink;

namespace QuickLinkTest
{
    class Program
    {
        public static async Task Main()
        {
            MessageType GenericMessage = MessageType.Get("GenericMessage");

            using (Host host = new Host(50240))
            using (Client client = new Client("127.0.0.1", 50240))
            {
                host.MessageReceived.Subscribe(GenericMessage, (MessageReader reader) => {
                    Console.WriteLine($" - Host received message: {reader.ReadString()}");
                });

                client.MessageReceived.Subscribe(GenericMessage, (MessageReader reader) => {
                    Console.WriteLine($" - Client received message: {reader.ReadString()}");
                });

                await Task.Delay(1000);

                using(MessageWriter writer = new MessageWriter(GenericMessage))
                {
                    writer.WriteString("Hello clients from server!");

                    host.Server.BroadcastMessage(writer);
                }

                await Task.Delay(2000);

                Console.WriteLine(" - Closing host and client");
            }
        }
    }
}