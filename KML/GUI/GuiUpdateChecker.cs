using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace KML
{
    /// <summary>
    /// Manages checking for updates on GitHub
    /// </summary>
    public class GuiUpdateChecker
    {
        // TODO GuiUpdateChecker: Change GET_URL to ".../releases/latest" once there is a non-preview release
        private const string GET_URL = "https://api.github.com/repos/my-th-os/KML/releases";
        // Without giving header info, we'll get "403 forbidden"
        private const string HEADER_KEY = "User-Agent";
        private const string HEADER_VALUE = "KML";
        // The keys we want the values of
        private const string TAG_KEY = "tag_name";
        private const string GO_URL_KEY = "html_url";

        public static void CheckUpdate(object linkobj)
        {
            if (!(linkobj is Hyperlink))
                return;
            Hyperlink link = (Hyperlink)linkobj;

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

                    // Get assembly version, all four numbers
                    var assembly = System.Reflection.Assembly.GetEntryAssembly();
                    Version localVersion = assembly.GetName().Version;

                    if (remoteVersion.CompareTo(localVersion) > 0)
                    {
                        // Show the link
                        link.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            link.Inlines.Clear();
                            link.Inlines.Add("Version " + tag.Substring(1) + " available!");
                            link.NavigateUri = new Uri(goUrl);
                            link.IsEnabled = true;
                        }));
                    }
                }
            try
            {
            }
            catch (Exception)
            {
                ;
            }
        }

        public static void CheckAsThread(Hyperlink link)
        {
            var thread = new Task(CheckUpdate, link);
            thread.Start();
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
