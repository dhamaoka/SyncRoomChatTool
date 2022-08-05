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
        public string voiceBoxPath;
        public string voiceBoxAddress;

        public AppConfig()
        {
            InitializeComponent();
            numWait.Focus();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            waitTiming = numWait.Value;
            linkWaveFilePath = textBox1.Text;
            Close();
        }

        private void AppConfig_Load(object sender, EventArgs e)
        {
            numWait.Value = App.Default.waitTiming;
            textBox1.Text = App.Default.linkWaveFilePath;
            textBox2.Text = App.Default.VoiceBoxPath;
            if (string.IsNullOrEmpty(App.Default.VoiceBoxAddress))
            {
                textBox3.Text = "http://localhost:50021";
                App.Default.VoiceBoxAddress = "http://localhost:50021";
                App.Default.Save();
            }
            else
            {
                textBox3.Text = App.Default.VoiceBoxAddress;
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            string appPath = Application.StartupPath;
            string absolutePath = Path.Combine(appPath, "link.wav");

            OpenFileDialog dlg = new OpenFileDialog();
            
            dlg.FileName = absolutePath;
            dlg.Title = "リンク専用の音声ファイルを選択してください。";
            dlg.InitialDirectory = appPath;
            dlg.Filter = "音声ファイル|*.wav";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                linkWaveFilePath = dlg.FileName;
                textBox1.Text = linkWaveFilePath;
                dlg.Dispose();
            }
        }

        private void AppConfig_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
            {
                if (!string.IsNullOrEmpty(linkWaveFilePath)) 
                {
                    if (!File.Exists(linkWaveFilePath))
                    {
                        e.Cancel = true;
                        MessageBox.Show("ファイルが見つかりません。", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        return;
                    }
                }

                if (!string.IsNullOrEmpty(voiceBoxPath))
                {
                    if (!File.Exists(voiceBoxPath))
                    {
                        e.Cancel = true;
                        MessageBox.Show("ファイルが見つかりません。", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        return;
                    }
                }
            }
            Dispose();
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            linkWaveFilePath = textBox1.Text;
        }

        private void TextBox2_TextChanged(object sender, EventArgs e)
        {
            voiceBoxPath = textBox2.Text;   
        }

        private void TextBox3_TextChanged(object sender, EventArgs e)
        {
            voiceBoxAddress = textBox3.Text;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.FileName = "run.exe";
            dlg.Title = "VOICEBOXのEngineを指定します。";
            dlg.InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs\\VOICEVOX");
            dlg.Filter = "実行ファイル|*.exe";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                voiceBoxPath = dlg.FileName;
                textBox2.Text = voiceBoxPath;
                dlg.Dispose();
            }
        }
    }
}
