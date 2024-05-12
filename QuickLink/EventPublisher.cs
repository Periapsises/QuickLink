using System;
using System.Collections.Generic;

namespace QuickLink
{
    /// <summary>
    /// Represents an event publisher that allows subscribing to and publishing events.
    /// </summary>
    public class EventPublisher
    {
        private readonly List<Action> _callbacks = new List<Action>();

        /// <summary>
        /// Subscribes to the event.
        /// </summary>
        /// <param name="callback">The callback method to be invoked when the event is published.</param>
        public void Subscribe(Action callback)
        {
            _callbacks.Add(callback);
        }

        /// <summary>
        /// Unsubscribes from the event.
        /// </summary>
        /// <param name="callback">The callback method to be removed from the subscribers list.</param>
        public void Unsubscribe(Action callback)
        {
            _callbacks.Remove(callback);
        }

        /// <summary>
        /// Publishes the event, invoking all the subscribed callback methods.
        /// </summary>
        public void Publish()
        {
            foreach (var callback in _callbacks)
            {
                callback();
            }
        }
    }

    /// <summary>
    /// Represents an event publisher that allows subscribing to and publishing events with a specified argument type.
    /// </summary>
    /// <typeparam name="T">The type of the argument for the event.</typeparam>
    public class EventPublisher<T>
    {
        private readonly List<Action<T>> _callbacks = new List<Action<T>>();

        /// <summary>
        /// Subscribes to the event.
        /// </summary>
        /// <param name="callback">The callback method to be invoked when the event is published.</param>
        public void Subscribe(Action<T> callback)
        {
            _callbacks.Add(callback);
        }

        /// <summary>
        /// Unsubscribes from the event.
        /// </summary>
        /// <param name="callback">The callback method to be removed from the subscribers list.</param>
        public void Unsubscribe(Action<T> callback)
        {
            _callbacks.Remove(callback);
        }

        /// <summary>
        /// Publishes the event with the specified message, invoking all the subscribed callback methods.
        /// </summary>
        /// <param name="value">The value to be passed to the callback methods.</param>
        public void Publish(T value)
        {
            foreach (var callback in _callbacks)
            {
                callback(value);
            }
        }
    }
}