using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Forms;
using System.Speech.Synthesis;
using System.Media;
using System.IO;
using System.Net;

namespace SyncRoomChatTool
{
    public partial class Form1 : Form
    {
        enum CommentDivider : int
        {
            Comment = 1,
            UserName = 2
        }

        public Form1()
        {
            InitializeComponent();
        }

        private static UIAutomationLib ui = new UIAutomationLib();

        private static readonly string voiceVoxDefaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs\\VOICEVOX\\run.exe");

        private void Form1_Load(object sender, EventArgs e)
        {
            Menu_EnebleSpeech.Checked = App.Default.canSpeech;
            Menu_UseVoiceBox.Checked = App.Default.UseVoiceBox;
            richTextBox1.LanguageOption = RichTextBoxLanguageOptions.UIFonts;
            richTextBox1.Font = App.Default.logFont;
            richTextBox1.ZoomFactor = App.Default.zoomFacter;
            if (App.Default.waitTiming < 100)
            {
                App.Default.waitTiming = 100;
            }

            //VOICEVOXのパス設定がされていなくて
            if (String.IsNullOrEmpty( App.Default.VoiceBoxPath))
            {
                //VOICEVOXデフォルトパスにRun.exeが居る＝インストールされているとみなし、
                if (File.Exists(voiceVoxDefaultPath))
                {
                    //設定に保存する＝VOICEVOXが使えると見なす。
                    App.Default.VoiceBoxPath = voiceVoxDefaultPath;
                }
            }

            //存在しないリンクが貼られてた際の固定音声ファイルが指定されている＝裏で直接コンフィグファイルをイジった想定
            if (!File.Exists(App.Default.linkWaveFilePath))
            {
                //固定ファイルなしとする。
                App.Default.linkWaveFilePath = "";
            }

            if (string.IsNullOrEmpty(App.Default.VoiceBoxAddress))
            {
                App.Default.VoiceBoxAddress = "http://localhost:50021";
            }

            App.Default.Save();

            this.Size = App.Default.windowSize;

            _ = CheckProcess("SYNCROOM", this.statusStrip1, this.richTextBox1);
        }

        static async Task CheckProcess(string ProcessName, StatusStrip ststp, RichTextBox logView)
        {
            /*
             * 別タスクに、フォームのコントロールを渡して内容を変更するのは、お作法的にはダメなんだろうなぁ…と思いつつ。
             */

            //初期化。前回取得のログ全部と、最後のコメントの保管場所
            string oldLog = "";
            string lastComment = "";

            while (true)
            {
                TargetProcess tp = new TargetProcess(ProcessName);  //結構プロセス生きてるか調べるかなと思って別クラスにしたけど、監視タスク内でしか見てねぇ。
                await Task.Delay((int)App.Default.waitTiming);

                string toolMessage;

                if (tp.IsAlive())
                {
                    toolMessage = "は起動されています。";
                    try
                    {
                        AutomationElement synroomElement = ui.GetMainFrameElement(tp.Proc());

                        IntPtr chatwHnd = ui.FindHWndByCaptionAndProcessID("チャット", tp.Id());

                        if (chatwHnd != IntPtr.Zero)
                        {
                            AutomationElement chatWindow = AutomationElement.FromHandle(chatwHnd);
                            chatWindow.SetFocus();
                            ststp.Items[1].Text = "チャットログ監視中。";

                            AutomationElement chatLog = ui.GetElement(synroomElement);
                            ValuePattern vp = ui.GetValuePattern(chatLog);

                            if (vp == null) { break;}

                            Match match;
                            match = Regex.Match(chatLog.Current.Name, "^チャットログ");
                            if (match.Success)
                            {
                                if (oldLog == "")
                                {
                                    //基本初回のみ。
                                    oldLog = vp.Current.Value;
                                    logView.Text = oldLog;
                                }

                                //チャットログに変化があった場合。
                                if (oldLog != vp.Current.Value)
                                {
                                    oldLog = vp.Current.Value;
                                    string newComment = "";
                                    logView.Text = vp.Current.Value;

                                    //改行で区切って、一番最後を最新コメントとする。
                                    string[] ary = vp.Current.Value.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

                                    newComment = ary[ary.Count() - 1];

                                    if (App.Default.canSpeech)
                                    {
                                        //ユーザ名の取得
                                        string userName = GetComment(newComment, CommentDivider.UserName);

                                        string commentText = "";
                                        //リンクの場合
                                        if (IsLink(newComment))
                                        {
                                            commentText = "リンクが張られました。";
                                            if (File.Exists(App.Default.linkWaveFilePath))
                                            {
                                                SoundPlayer player = new SoundPlayer(App.Default.linkWaveFilePath);
                                                //非同期再生する
                                                player.Play();
                                                lastComment = newComment;
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            //コメント部分の取得
                                            commentText = GetComment(newComment, CommentDivider.Comment);
                                        }


                                        //空行でない & 連続同一コメントでないこと。
                                        if (IsBlank(newComment) == false && lastComment != newComment)
                                        {

                                            //日本語が含まれているか（ちょっと判定は微妙だけど）
                                            int lang = 0;
                                            if (UseOnlyEnglish(newComment))
                                            {
                                                //Ziraがしゃべる。VOICEVOXを使うとしてもだ。
                                                lang = 1;
                                            }
                                            //ToDo:リンクじゃないコメントで、コロンで区切った最後のコメントが来る。これ以降にあれやこれや入れる。


                                            //音声用の別処理をぶっ込む。
                                            SpeechSynthe(commentText,lang);
                                        }
                                    }

                                    lastComment = newComment;
                                }
                            }
                        }
                        else
                        {
                            ststp.Items[1].Text = "チャットログ待機中…";
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message.ToString());
                    }
                }
                else
                {
                    toolMessage = "は起動されていません。";
                    oldLog = "";
                    lastComment = "";
                }
                ststp.Items[0].Text = ProcessName + toolMessage;
            }
        }

        private static string GetComment(string newComment, CommentDivider dv)
        {
            //コロンで区切る。
            string[] ary = newComment.Split(new string[] { ":" }, StringSplitOptions.None);

            if (ary.Length < 2)
            {
                return newComment;
            }
            else
            {
                return ary[ary.Length - (int)dv];
            }
        }

        private static bool IsBlank(string newComment)
        {
            //末尾がコロンかどうか。空行チェック。
            if (newComment.Substring(newComment.Length-1,1)==":")
            {
                return true;
            }
            return false;
        }

        private static bool IsLink(string newComment)
        {
            //リンクかどうかのチェック
            Match match;
            match = Regex.Match(newComment, "https?://");
            if (match.Success)
            {
                return true;
            }
            return false;
        }

        private static bool UseOnlyEnglish(string newComment)
        {
            //英数のみかのチェックというか、指定のワードが入ってるかどうか（主に日本語）
            Match match;
            match = Regex.Match(newComment, "[ぁ-んァ-ヶｱ-ﾝﾞﾟ一-龠！-／：-＠［-｀｛-～、-〜”’・]");
            if (match.Success)
            {
                return false;
            }
            return true;
        }

        private static void SpeechSynthe(string commentText, int lang = 0)
        {
            string speechComment = commentText;

            //メッセージ（情報）を鳴らす
            SystemSounds.Asterisk.Play();

            if (App.Default.UseVoiceBox == true && lang == 0)
            {
                SpeechVOICEVOX(commentText);
                return;
            }

            SpeechSynthesizer synth = new SpeechSynthesizer
            {
                Rate = -1
            };

            //lang == 0 日本語でしゃべる。VOICEVOXを使うかどうかで処理分岐。
            if (lang == 0)
            {
                synth.SelectVoice("Microsoft Haruka Desktop");
            }
            else
            {
                synth.SelectVoice("Microsoft Zira Desktop");
            }

            synth.SpeakAsync(speechComment);
        }


        private static void SpeechVOICEVOX(string commentText)
        {
            string baseUrl = App.Default.VoiceBoxAddress;
            string url;

            if (string.IsNullOrEmpty(baseUrl))
            {
                Debug.WriteLine("URLが入ってない");
                return;
            }

            if (baseUrl.Substring(baseUrl.Length - 1, 1) != "/")
            {
                baseUrl += "/";
            }

            url = baseUrl + $"audio_query?text='{commentText}'&speaker=2";

            var client = new ServiceHttpClient(url);
            String QueryResponce = "";
            var ret = client.Post(ref QueryResponce, "");

            if (ret.StatusCode.Equals(HttpStatusCode.OK))
            {
                url = baseUrl + $"synthesis?speaker=2";
                client = new ServiceHttpClient(url);
                string wavFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                ret = client.Post(ref QueryResponce, wavFile);
                if (ret.StatusCode.Equals(HttpStatusCode.OK))
                {
                    SoundPlayer player = new SoundPlayer(wavFile);
                    //非同期再生する
                    player.Play();
                }
            }
        }

        private void MenuClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void RichTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            // クリックされたリンクをWebブラウザで開く
            Process.Start(e.LinkText);
        }

        private void MenuOption_Click(object sender, EventArgs e)
        {
            AppConfig apConf = new AppConfig();
            if (apConf.ShowDialog() == DialogResult.OK)
            {
                App.Default.linkWaveFilePath = apConf.linkWaveFilePath;
                App.Default.waitTiming = apConf.waitTiming;
                App.Default.VoiceBoxAddress = apConf.voiceBoxAddress;
                App.Default.VoiceBoxPath = apConf.voiceBoxPath;
                App.Default.Save();
            }
        }

        private void MenuFont_Click(object sender, EventArgs e)
        {

            fontDialog1.Font = richTextBox1.Font;
            DialogResult dr = fontDialog1.ShowDialog();

            if (dr == DialogResult.OK)
            {
                richTextBox1.Font = fontDialog1.Font;
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            App.Default.zoomFacter = richTextBox1.ZoomFactor;
            App.Default.logFont = richTextBox1.Font;
            App.Default.windowSize = this.Size;
            App.Default.Save();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            //カレット位置を末尾に移動
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            //カレット位置までスクロール
            richTextBox1.ScrollToCaret();
        }

        private void Menu_EnebleSpeech_Click(object sender, EventArgs e)
        {
            App.Default.canSpeech = Menu_EnebleSpeech.Checked;
            App.Default.Save();
        }

        private void Menu_VoiceBox_Click(object sender, EventArgs e)
        {
            App.Default.UseVoiceBox = Menu_UseVoiceBox.Checked;
            App.Default.Save();
        }
    }
}
