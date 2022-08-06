using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace SyncRoomChatTool
{
    public partial class Help : Form
    {
        string url = "https://github.com/dhamaoka/SyncRoomChatTool/releases/latest";

        public Help()
        {
            InitializeComponent();
            InitializeAsync();
        }

        async void InitializeAsync()
        {
            try
            {
                await webView21.EnsureCoreWebView2Async(null);
            }
            catch (Exception)
            {
                DialogResult rs = MessageBox.Show(
                    "WebView2ランタイムが見つかりません。\n\nインストールしますか？",
                    Application.ProductName,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (rs == DialogResult.Yes)
                {
                    // WebView2ランタイムのダウンロード
                    string downloadUrl = "https://go.microsoft.com/fwlink/p/?LinkId=2124703"; // ←★ダウンロード先を指定
                    System.Reflection.Assembly myAssembly = System.Reflection.Assembly.GetEntryAssembly();
                    string tmpInstPath = Path.GetDirectoryName(myAssembly.Location) + @"\tmp";
                    string installerPath = tmpInstPath + @"\MicrosoftEdgeWebview2Setup.exe";
                    Directory.CreateDirectory(tmpInstPath);
                    System.Net.WebClient wc = new System.Net.WebClient();
                    wc.DownloadFile(downloadUrl, installerPath);
                    wc.Dispose();

                    // インストール実行
                    Process proc = new Process();
                    proc.StartInfo.FileName = installerPath;
                    proc.StartInfo.Arguments = @"/install";
                    proc.Start();
                    proc.WaitForExit();
                    System.IO.Directory.Delete(tmpInstPath, true);
                    Application.Restart();
                }
                this.Close();
            }
        }

        private void Help_Load(object sender, EventArgs e)
        {
            /*
            string appPath = Application.StartupPath;
            string url = Path.Combine(appPath, "help.html");

            StreamReader sr = new StreamReader(url);

            string text = sr.ReadToEnd();

            sr.Close();
            webBrowser1.DocumentText = text;

            webBrowser1.ScriptErrorsSuppressed = true;
            webBrowser1.DocumentText = "<!DOCTYPE html>";

            //string url = "https://github.com/dhamaoka/SyncRoomChatTool/releases/latest";
            webBrowser1.Navigate(url);
            */
        }

        private void webView21_CoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            webView21.CoreWebView2.Navigate(url);
        }
    }
}
