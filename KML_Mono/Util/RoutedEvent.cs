using System;

namespace System.Windows
{
    /// <summary>
    /// This usually is a WPF only event type
    /// But we don't need any special properties, so we fake it for Mono
    /// </summary>
    public class RoutedEventArgs : EventArgs
    {
    }

    /// <summary>
    /// This usually is a WPF only event type
    /// But we don't need any special properties, so we fake it for Mono
    /// </summary>
    public delegate void RoutedEventHandler(object sender, RoutedEventArgs e);
}
