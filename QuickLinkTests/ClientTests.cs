using System.Net.Sockets;
using QuickLink;

namespace QuickLinkTests;

public class ClientTests
{
    [Fact(DisplayName = "Client throws a SocketException if the provided host cannot be reached.")]
    public void ClientThrowsSocketExceptionIfServerIsDown()
    {
        Assert.ThrowsAsync<SocketException>(async () => {
            try
            {
                using (var client = new Client())
                {
                    await client.Connect("testing.invalid", 0);
                }
            }
            catch (SocketException e)
            {
                Assert.Equal(SocketError.HostNotFound, e.SocketErrorCode);
                throw;
            }
        });
    }

    [Fact(DisplayName = "Client throws an ArgumentOutOfRangeException if the provided port is outside the valid range.")]
    public void ClientThrowsArgumentOutOfRangeExceptionIfPortIsOutsideRange()
    {
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => {
            using (var client = new Client())
            {
                await client.Connect("localhost", -1);
            }
        });

        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => {
            using (var client = new Client())
            {
                await client.Connect("localhost", 65536);
            }
        });
    }
}