using System;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace KML
{
    /// <summary>
    /// Manages checking for updates on GitHub
    /// </summary>
    public class GuiUpdateChecker
    {
        /// <summary>
        /// Checks for update on GitHub by newest release tag compared to version from assembly
        /// </summary>
        /// <param name="linkobj">A Hyperlink Control to place the GitHub link in</param>
        public static void CheckUpdate(object linkobj)
        {
            if (!(linkobj is Hyperlink))
                return;
            Hyperlink link = (Hyperlink)linkobj;

            try
            {
                Tuple<Version, Uri> github = UpdateChecker.GetGitHubLatest();
                Version remoteVersion = github.Item1;
                Uri remoteLink = github.Item2;

                Version localVersion = UpdateChecker.GetAssemblyVersion();

                if (remoteVersion.CompareTo(localVersion) > 0)
                {
                    // Show the link
                    link.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        link.Inlines.Clear();
                        link.Inlines.Add("Version " + UpdateChecker.VersionToString(remoteVersion) + " available!");
                        link.NavigateUri = remoteLink;
                        link.IsEnabled = true;
                    }));
                }
            }
            catch (Exception)
            {
                ;
            }
        }

        /// <summary>
        /// Checks for update in a separate thread
        /// </summary>
        /// <param name="link">A Hyperlink Control to place the GitHub link in</param>
        public static void CheckAsThread(Hyperlink link)
        {
            var thread = new Task(CheckUpdate, link);
            thread.Start();
        }
    }
}
