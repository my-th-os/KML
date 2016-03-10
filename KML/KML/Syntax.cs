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
    class Syntax
    {
        /// <summary>
        /// This class encapsules all data of a message.
        /// </summary>
        public class Message
        {
            /// <summary>
            /// Get the parent of the KmlItem where the message came from.
            /// </summary>
            public KmlNode Parent { get; private set; }

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
            /// <param name="parent">The parent KmlNode from message source</param>
            /// <param name="source">The source KmlItem where the message came from</param>
            /// <param name="message">The message text</param>
            public Message(KmlNode parent, KmlItem source, string message)
            {
                Parent = parent;
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
                string s = "";
                if (Parent != null)
                {
                    s += Parent.ToString() + " -> ";
                }
                else
                {
                    s += "ROOT -> ";
                }
                s += Source.ToString();
                if (withNewLine)
                {
                    s += ":\n";
                }
                else
                {
                    s += ": ";
                }
                s += Text;
                return s;
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
            /// <param name="parent">The parent KmlNode from message source</param>
            /// <param name="source">The source KmlItem where the message came from</param>
            /// <param name="message">The message text</param>
            public WarningMessage(KmlNode parent, KmlItem source, string message)
                : base (parent, source, message)
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
            /// <param name="parent">The parent KmlNode from message source</param>
            /// <param name="source">The source KmlItem where the message came from</param>
            /// <param name="message">The message text</param>
            public ErrorMessage(KmlNode parent, KmlItem source, string message)
                : base(parent, source, message)
            {
            }
        }

        /// <summary>
        /// Get a list of all messages.
        /// </summary>
        public static List<Message> Messages = new List<Message>();

        /// <summary>
        /// Generate a warning message.
        /// </summary>
        /// <param name="parent">The parent KmlNode from message source</param>
        /// <param name="source">The source KmlItem where the message came from</param>
        /// <param name="message">The message text</param>
        public static void Warning(KmlNode parent, KmlItem source, string message)
        {
            Messages.Add(new WarningMessage(parent, source, message));
        }

        /// <summary>
        /// Generate a warning message.
        /// </summary>
        /// <param name="source">The source KmlNode where the message came from</param>
        /// <param name="message">The message text</param>
        public static void Warning(KmlNode source, string message)
        {
            Messages.Add(new WarningMessage(source.Parent, source, message));
        }

        /// <summary>
        /// Generate a error message.
        /// </summary>
        /// <param name="parent">The parent KmlNode from message source</param>
        /// <param name="source">The source KmlItem where the message came from</param>
        /// <param name="message">The message text</param>
        public static void Error(KmlNode parent, KmlItem source, string message)
        {
            Messages.Add(new ErrorMessage(parent, source, message));
        }

        /// <summary>
        /// Generate a error message.
        /// </summary>
        /// <param name="source">The source KmlNode where the message came from</param>
        /// <param name="message">The message text</param>
        public static void Error(KmlNode source, string message)
        {
            Messages.Add(new ErrorMessage(source.Parent, source, message));
        }
    }
}
