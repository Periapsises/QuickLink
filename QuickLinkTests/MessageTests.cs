using QuickLink;

namespace QuickLinkTests;

public class MessageTests
{
    public MessageType MessageType1 = MessageType.Get("MessageType1");
    public MessageType MessageType2 = MessageType.Get("MessageType2");
    public MessageType MessageTypeSameAs1 = MessageType.Get("MessageType1");

    [Fact(DisplayName = "A message publisher only fires the callbacks attached to the proper message type.")]
    public void MessagePublisherOnlyFiresCallbacksForMessageType()
    {
        MessagePublisher publisher = new MessagePublisher();
        bool message1Fired = false;
        bool message2Fired = false;

        publisher.Subscribe(MessageType1, (message) => { message1Fired = true; });
        publisher.Subscribe(MessageType2, (message) => { message2Fired = true; });

        using (MessageWriter writer = new MessageWriter(MessageType1))
        {
            MessageReader reader = writer.ToReader();
            publisher.Publish(reader);
        }

        Assert.True(message1Fired);
        Assert.False(message2Fired);
    }

    [Fact(DisplayName = "Message types gotten with the same name must attach a callback to the same underlying MessageType.")]
    public void MessageTypesWithSameNameAreEqualAndAttachProperly()
    {
        Assert.Equal(MessageType1, MessageTypeSameAs1);

        MessagePublisher publisher = new MessagePublisher();
        int message1FiredCount = 0;
        int message2FiredCount = 0;

        publisher.Subscribe(MessageType1, (message) => { message1FiredCount++; });
        publisher.Subscribe(MessageTypeSameAs1, (message) => { message2FiredCount++; });

        using (MessageWriter writer = new MessageWriter(MessageType1))
        {
            MessageReader reader = writer.ToReader();
            publisher.Publish(reader);
        }

        using (MessageWriter writer = new MessageWriter(MessageTypeSameAs1))
        {
            MessageReader reader = writer.ToReader();
            publisher.Publish(reader);
        }

        Assert.Equal(2, message1FiredCount);
        Assert.Equal(2, message2FiredCount);
    }

    [Fact(DisplayName = "Messages can properly write and read different types of data.")]
    public void MessagesCanWriteAndReadDifferentDataTypes()
    {
        using (MessageWriter writer = new MessageWriter(MessageType1))
        {
            byte[] testBytes = {0x48, 0x65, 0x6c, 0x6c, 0x6f, 0x2c, 0x20, 0x77, 0x6f, 0x72, 0x6c, 0x64, 0x21};

            writer.WriteBool(true);
            writer.WriteBool(false);
            writer.WriteByte(0x01);
            writer.WriteInt16(0x0203);
            writer.WriteInt32(0x04050607);
            writer.WriteFloat(1.23f);
            writer.WriteDouble(1.23);
            writer.WriteString("Hello, world!");
            writer.WriteBytes(testBytes, 0, testBytes.Length);

            byte[] resultBytes = new byte[testBytes.Length];

            MessageReader reader = writer.ToReader();
            Assert.True(reader.ReadBool());
            Assert.False(reader.ReadBool());
            Assert.Equal(0x01, reader.ReadByte());
            Assert.Equal(0x0203, reader.ReadInt16());
            Assert.Equal(0x04050607, reader.ReadInt32());
            Assert.Equal(1.23f, reader.ReadFloat());
            Assert.Equal(1.23, reader.ReadDouble());
            Assert.Equal("Hello, world!", reader.ReadString());
            reader.ReadBytes(resultBytes, 0, resultBytes.Length);

            for (int i = 0; i < testBytes.Length; i++)
            {
                Assert.Equal(testBytes[i], resultBytes[i]);
            }
        }
    }
}