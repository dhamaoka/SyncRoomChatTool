using System;
using System.IO;
using System.Media;
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
        public double volume;
        public bool playAsync;

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
            //ここの変数代入いらなくね？呼び出し側で、App.Defaultに入れてるんじゃないかな？
            waitTiming = numWait.Value;
            cutLength = numCut.Value;
            linkWaveFilePath = textBox1.Text;
            playAsync = checkBox1.Checked;
            Close();
        }

        private void AppConfig_Load(object sender, EventArgs e)
        {
            if (App.Default.waitTiming < 500)
            {
                App.Default.waitTiming = 500;
            }
            numWait.Value = App.Default.waitTiming;
            numCut.Value = App.Default.cutLength;
            textBox1.Text = App.Default.linkWaveFilePath;
            textBox2.Text = App.Default.VoiceVoxPath;
            checkBox1.Checked = App.Default.PlayAsync;
            try
            {
                volume = App.Default.Volume;
                trackBar1.Value = (int)Math.Floor(volume * 10);
                numericUpDown1.Value = (decimal)volume;
            }
            catch
            {
                trackBar1.Value = 1;
                numericUpDown1.Value = 1;
            }
            if (string.IsNullOrEmpty(App.Default.VoiceVoxAddress))
            {
                textBox3.Text = "http://127.0.0.1:50021";
                App.Default.VoiceVoxAddress = "http://127.0.0.1:50021";
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

            OpenFileDialog dlg = new OpenFileDialog
            {
                FileName = absolutePath,
                Title = "リンク専用の音声ファイルを選択してください。",
                InitialDirectory = appPath,
                Filter = "音声ファイル|*.wav"
            };

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

        private void Button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                FileName = "run.exe",
                Title = "VOICEVOXのEngineを指定します。",
                InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs\\VOICEVOX"),
                Filter = "実行ファイル|*.exe"
            };

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                voiceVoxPath = dlg.FileName;
                textBox2.Text = voiceVoxPath;
                dlg.Dispose();
            }
        }

        private void ButtonPlay_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.Text))
            {
                SoundPlayer player = new SoundPlayer(textBox1.Text);
                //非同期再生する
                player.Play();
            }
        }

        private void TrackBar1_ValueChanged(object sender, EventArgs e)
        {
            volume = (double)trackBar1.Value / 10;
            numericUpDown1.Value = (decimal)volume;
        }

        private void NumericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            trackBar1.Value = (int)((double)numericUpDown1.Value * 10);
        }
    }
}
