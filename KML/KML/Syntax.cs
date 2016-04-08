using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KML
{
    /// <summary>
    /// This class captures warnings and errors from within the KML
    /// generation and identification.
    /// The message classes are capsulated within this class.
    /// </summary>
    public class Syntax
    {
        /// <summary>
        /// This class encapsules all data of a message.
        /// </summary>
        public class Message
        {
            /// <summary>
            /// Get the source KmlItem where the message came from.
            /// </summary>
            public KmlItem Source { get; private set; }

            /// <summary>
            /// Get the message text.
            /// </summary>
            public string Text { get; private set; }

            /// <summary>
            /// Creates a new message.
            /// </summary>
            /// <param name="source">The source KmlItem where the message came from</param>
            /// <param name="message">The message text</param>
            public Message(KmlItem source, string message)
            {
                Source = source;
                Text = message;
            }

            /// <summary>
            /// Get display text for the message. Parent and source data is prefixed.
            /// </summary>
            /// <param name="withNewLine">Do you want a new line after parent and source readout and before the message?</param>
            /// <returns>A display string</returns>
            public string ToString(bool withNewLine)
            {
                StringBuilder s = new StringBuilder();
                if (Source.Parent != null)
                {
                    s.Append(Source.Parent.ToString());
                    s.Append(" -> ");
                }
                else
                {
                    s.Append("ROOT -> ");
                }
                s.Append(Source.ToString());
                if (withNewLine)
                {
                    s.Append(":\n");
                }
                else
                {
                    s.Append(": ");
                }
                s.Append(Text);
                return s.ToString();
            }

            /// <summary>
            /// Get display text for the message. Parent and source data is prefixed.
            /// </summary>
            /// <returns>A display string</returns>
            public override string ToString()
            {
                return ToString(false);
            }
        }

        /// <summary>
        /// A derived message class for warnings.
        /// </summary>
        public class WarningMessage : Message
        {
            /// <summary>
            /// Creates a new warning message.
            /// </summary>
            /// <param name="source">The source KmlItem where the message came from</param>
            /// <param name="message">The message text</param>
            public WarningMessage(KmlItem source, string message)
                : base (source, message)
            {
            }
        }

        /// <summary>
        /// A derived message class for errors.
        /// </summary>
        public class ErrorMessage : Message
        {
            /// <summary>
            /// Creates a new error message.
            /// </summary>
            /// <param name="source">The source KmlItem where the message came from</param>
            /// <param name="message">The message text</param>
            public ErrorMessage(KmlItem source, string message)
                : base(source, message)
            {
            }
        }

        /// <summary>
        /// Get a list of all messages.
        /// </summary>
        public static List<Message> Messages = new List<Message>();

        /// <summary>
        /// Generate a info message.
        /// </summary>
        /// <param name="source">The source KmlNode where the message came from</param>
        /// <param name="message">The message text</param>
        public static void Info(KmlItem source, string message)
        {
            Messages.Add(new Message(source, message));
        }

        /// <summary>
        /// Generate a warning message.
        /// </summary>
        /// <param name="source">The source KmlNode where the message came from</param>
        /// <param name="message">The message text</param>
        public static void Warning(KmlItem source, string message)
        {
            Messages.Add(new WarningMessage(source, message));
        }

        /// <summary>
        /// Generate a error message.
        /// </summary>
        /// <param name="source">The source KmlNode where the message came from</param>
        /// <param name="message">The message text</param>
        public static void Error(KmlItem source, string message)
        {
            Messages.Add(new ErrorMessage(source, message));
        }
    }
}
