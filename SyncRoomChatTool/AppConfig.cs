using System;
using System.IO;
using System.Windows.Forms;

namespace SyncRoomChatTool
{
    public partial class AppConfig : Form
    {
        public decimal waitTiming;
        public decimal cutLength;
        public string linkWaveFilePath;
        public string voiceVoxPath;
        public string voiceVoxAddress;

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
            cutLength = numCut.Value;
            linkWaveFilePath = textBox1.Text;
            Close();
        }

        private void AppConfig_Load(object sender, EventArgs e)
        {
            numWait.Value = App.Default.waitTiming;
            numCut.Value = App.Default.cutLength;
            textBox1.Text = App.Default.linkWaveFilePath;
            textBox2.Text = App.Default.VoiceVoxPath;
            if (string.IsNullOrEmpty(App.Default.VoiceVoxAddress))
            {
                textBox3.Text = "http://localhost:50021";
                App.Default.VoiceVoxAddress = "http://localhost:50021";
                App.Default.Save();
            }
            else
            {
                textBox3.Text = App.Default.VoiceVoxAddress;
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

                if (!string.IsNullOrEmpty(voiceVoxPath))
                {
                    if (!File.Exists(voiceVoxPath))
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
            voiceVoxPath = textBox2.Text;
        }

        private void TextBox3_TextChanged(object sender, EventArgs e)
        {
            voiceVoxAddress = textBox3.Text;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.FileName = "run.exe";
            dlg.Title = "VOICEVOXのEngineを指定します。";
            dlg.InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs\\VOICEVOX");
            dlg.Filter = "実行ファイル|*.exe";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                voiceVoxPath = dlg.FileName;
                textBox2.Text = voiceVoxPath;
                dlg.Dispose();
            }
        }
    }
}
