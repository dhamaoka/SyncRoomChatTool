using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace SyncRoomChatTool
{
    public partial class AppConfig : Form
    {
        public decimal waitTiming;
        public string linkWaveFilePath;

        public AppConfig()
        {
            InitializeComponent();
            numWait.Focus();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            waitTiming = numWait.Value;
            linkWaveFilePath = textBox1.Text;
            this.Close();
        }

        private void AppConfig_Load(object sender, EventArgs e)
        {
            this.numWait.Value = App.Default.waitTiming;
            this.textBox1.Text = App.Default.linkWaveFilePath;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            string appPath = System.Windows.Forms.Application.StartupPath;
            string absolutePath = Path.Combine(appPath, "link.wav");

            OpenFileDialog dlg = new OpenFileDialog();
            
            dlg.FileName = absolutePath;
            dlg.Title = "リンク専用の音声ファイルを選択してください。";
            dlg.InitialDirectory = appPath;
            dlg.Filter = "音声ファイル|*.wav";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                this.linkWaveFilePath = dlg.FileName;
                textBox1.Text = this.linkWaveFilePath;
                dlg.Dispose();
            }
        }

        private void AppConfig_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult == DialogResult.OK)
            {
                if (string.IsNullOrEmpty(this.linkWaveFilePath))
                {
                    this.Dispose();
                }
                else
                {
                    if (!File.Exists(linkWaveFilePath))
                    {
                        e.Cancel = true;
                        MessageBox.Show("ファイルが見つかりません。", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    }
                    else
                    {
                        this.Dispose();
                    }
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            this.linkWaveFilePath = textBox1.Text;
        }
    }
}
