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
}