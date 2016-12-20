using System;
using System.ComponentModel;

namespace CH2.MVVM
{
    /// <summary>
    /// Extensions for working with events
    /// </summary>
    public static class EventExtensions
    {
        /// <summary>
        /// Safely raises the given event
        /// </summary>
        public static void Raise(this EventHandler eventToRaise, object sender, EventArgs eventArgs)
        {
            eventToRaise?.Invoke(sender, eventArgs);
        }

        /// <summary>
        /// Safely raises the given event
        /// </summary>
        public static void Raise<TEventArgs>(this EventHandler<TEventArgs> eventToRaise, object sender, TEventArgs eventArgs)
            where TEventArgs : EventArgs
        {
            eventToRaise?.Invoke(sender, eventArgs);
        }

        /// <summary>
        /// Safely raises the given event
        /// </summary>
        public static void Raise(this PropertyChangedEventHandler eventToRaise, object sender, PropertyChangedEventArgs eventArgs)
        {
            eventToRaise?.Invoke(sender, eventArgs);
        }

    }
}
