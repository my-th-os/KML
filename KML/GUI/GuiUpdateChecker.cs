using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace KML
{
    /// <summary>
    /// Manages checking for updates on GitHub
    /// </summary>
    public class GuiUpdateChecker
    {
        // TODO GuiUpdateChecker: Change GET_URL to ".../releases/latest" once there is a non-pre-release release
        private const string GET_URL = "https://api.github.com/repos/my-th-os/KML/releases";
        // Without giving header info, we'll get "403 forbidden"
        private const string HEADER_KEY = "User-Agent";
        private const string HEADER_VALUE = "KML";
        // The keys we want the values of
        private const string TAG_KEY = "tag_name";
        private const string GO_URL_KEY = "html_url";

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
                Tuple<Version, Uri> github = GetGitHubLatest();
                Version remoteVersion = github.Item1;
                Uri remoteLink = github.Item2;

                Version localVersion = GetAssemblyVersion();

                if (remoteVersion.CompareTo(localVersion) > 0)
                {
                    // Show the link
                    link.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        link.Inlines.Clear();
                        link.Inlines.Add("Version " + VersionToString(remoteVersion) + " available!");
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

        /// <summary>
        /// Get local version from assembly
        /// </summary>
        /// <returns>Assembly version number</returns>
        public static Version GetAssemblyVersion()
        {
            // Get assembly version, all four numbers
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            return assembly.GetName().Version;
        }

        /// <summary>
        /// Check GitHub API for latest version information.
        /// This is public to have a test that may fail if GitHub changes the API.
        /// </summary>
        /// <returns>Latest version number and GitHub-link to this release</returns>
        public static Tuple<Version, Uri> GetGitHubLatest()
        {
            using (WebClient client = new WebClient())
            {
                client.Headers.Add(HEADER_KEY, HEADER_VALUE);
                string json = client.DownloadString(GET_URL);
                // Check for newer version by comparing version to content of "tag_name"
                string tag = GetValue(json, TAG_KEY);
                // Users should go to content of "html_url"
                string goUrl = GetValue(json, GO_URL_KEY);

                // Tag starts with "v", version doesn't
                string v = tag.Substring(1);
                // Need to have four numbers / three dots otherwise they default to -1
                for (int i = v.Count(c => c == '.'); i < 3; i++)
                    v += ".0";
                Version remoteVersion = Version.Parse(v);

                Uri remoteLink = new Uri(goUrl);

                return new Tuple<Version, Uri>(remoteVersion, remoteLink);
            }
        }

        /// <summary>
        /// Format the version number to display in wanted format.
        /// Version instance would always have four numbers, here omit the trailing ".0"
        /// </summary>
        /// <param name="version">The Version to format</param>
        /// <returns>Display version string</returns>
        public static string VersionToString(Version version)
        {
            string v = version.ToString();
            while (v.EndsWith(".0"))
                v = v.Substring(0, v.Length - 2);
            return v;
        }

        private static string GetValue(string json, string key)
        {
            // Just search for first occurence of key strings, no real json parsing
            string prefix = @"""" + key + @""":""";
            string suffix = @""",";
            int begin = json.IndexOf(prefix);
            int end = json.IndexOf(suffix, begin + prefix.Length);
            if (begin < 0 || end < 0) 
                return "";
            return json.Substring(begin + prefix.Length, end - begin - prefix.Length);
        }
    }
}
