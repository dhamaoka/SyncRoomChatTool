using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SyncRoomChatTool
{
    public partial class Help : Form
    {
        public Help()
        {
            InitializeComponent();
        }

        private void Help_Load(object sender, EventArgs e)
        {
            string appPath = Application.StartupPath;
            string absolutePath = Path.Combine(appPath, "help.html");

            StreamReader sr = new StreamReader(absolutePath);

            string text = sr.ReadToEnd();

            sr.Close();
            webBrowser1.DocumentText = text;
            webBrowser1.Navigate(absolutePath);
        }
    }
}
