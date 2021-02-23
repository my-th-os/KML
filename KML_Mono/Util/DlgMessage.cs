using System;

namespace KML
{
    /// <summary>
    /// The WPF app shows some dialog boxes, that we don't want in Mono CLI
    /// </summary>
    public class DlgMessage
    {
        public static void Show(string message)
        {
            Console.WriteLine(message);
        }
    }
}
