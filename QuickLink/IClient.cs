using System.Threading.Tasks;

namespace QuickLink
{
    /// <summary>
    /// Represents a client that can send messages to a server.
    /// </summary>
    public interface IClient
    {
        /// <summary>
        /// Event that is raised when a message is received from the server.
        /// </summary>
        MessagePublisher MessageReceived { get; }

        /// <summary>
        /// Sends a message to the server.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        void SendToServer(MessageWriter message);
    }
}