using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Reflection;
using System.Security.Policy;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Forms;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SyncRoomChatTool
{
    public partial class Form1 : Form
    {
        enum CommentDivider : int
        {
            Comment = 1,
            UserName = 2
        }

        private static readonly int commentLimit = (int)App.Default.cutLength;

        public Form1()
        {
            InitializeComponent();
        }


        private class TwitCastingUserInfo
        {
            public string Supporter_count { get; set; }
            public string Supporting_count { get; set; }
            [JsonProperty("user")]
            public User UserInfo { get;set;}
        }

        public class User
        {
            [JsonProperty("id")]
            public string Id { get; set; }
            [JsonProperty("screen_id")]
            public string Screen_id { get; set; }
            [JsonProperty("name")]
            public string Name { get; set; }
            [JsonProperty("image")]
            public string Image { get; set; }
            [JsonProperty("profile")]
            public string Profile { get; set; }
            [JsonProperty("level")]
            public int Level { get; set; }
            [JsonProperty("last_movie_id")]
            public string Last_movie_id { get; set; }
            [JsonProperty("is_live")]
            public bool Is_live { get; set; }
            [JsonProperty("Supporter_count")]
            public int Supporter_count { get; set; }
            [JsonProperty("supporting_count")]
            public int Supporting_count { get; set; }
            [JsonProperty("created")]
            public int Created { get; set; }
        }

        public class TwitCastingComment
        {
            [JsonProperty("id")]
            public string Id { get; set; }
            [JsonProperty("message")]
            public string Message { get; set; }
            [JsonProperty("created")]
            public int Created { get; set; }
            [JsonProperty("from_user")]
            public User FromUser { get; set; }
        }

        public class TwitCastingCommentRoot
        {
            [JsonProperty("movie_id")]
            public string MovieId { get; set; }
            [JsonProperty("all_count")]
            public int AllCount { get; set; }
            [JsonProperty("comments")]
            public List<TwitCastingComment> Comments { get; set; }
        }

        private class SpeakerFromAPI
        {
            public string Name { get; set; }
            public string speaker_uuid { get; set; }
            public List<StyleFromAPI> styles { get; set; }
            public string version { get; set; }
        }

        private class StyleFromAPI
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        private class Speaker
        {
            public int StyleId { get; set; }
            public string UserName { get; set; }
            public bool ChimeFlg { get; set; }
            public bool SpeechFlg { get; set; }
        }

        private static UIAutomationLib ui = new UIAutomationLib();
        private static readonly string voiceVoxDefaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs\\VOICEVOX\\run.exe");

        static List<Speaker> UserTable = new List<Speaker> { };
        static List<Speaker> StyleDef = new List<Speaker> { };

        private async void Form1_Load(object sender, EventArgs e)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

            this.Text += $" {fileVersionInfo.ProductVersion}";

            MenuEnableTwitcasting.Checked = App.Default.useTwitcasting;
            MenuEnebleSpeech.Checked = App.Default.canSpeech;
            MenuUseVoiceVox.Checked = App.Default.UseVoiceVox;
            richTextBox1.LanguageOption = RichTextBoxLanguageOptions.UIFonts;
            richTextBox1.Font = App.Default.logFont;
            richTextBox1.ZoomFactor = App.Default.zoomFacter;
            if (App.Default.waitTiming < 950)
            {
                App.Default.waitTiming = 950;
            }
            if (App.Default.cutLength < 20)
            {
                App.Default.cutLength = 20;
            }

            //VOICEVOXのパス設定がされていなくて（初回起動時想定。デフォルトコンフィグは空なので）
            if (String.IsNullOrEmpty(App.Default.VoiceVoxPath))
            {
                //VOICEVOXデフォルトパスにRun.exeが居る＝インストールされているとみなし、
                if (File.Exists(voiceVoxDefaultPath))
                {
                    //設定に保存する＝VOICEVOXが使えると見なす。
                    App.Default.VoiceVoxPath = voiceVoxDefaultPath;
                }
            }

            //存在しないリンクが貼られてた際の固定音声ファイルが指定されている＝裏で直接コンフィグファイルをイジった想定
            if (!File.Exists(App.Default.linkWaveFilePath))
            {
                //固定ファイルなしとする。
                App.Default.linkWaveFilePath = "";
            }

            if (string.IsNullOrEmpty(App.Default.VoiceVoxAddress))
            {
                App.Default.VoiceVoxAddress = "http://localhost:50021";
            }

            App.Default.Save();

            this.Size = App.Default.windowSize;
            this.Refresh();
            richTextBox1.Refresh();

            TargetProcess tp = new TargetProcess("run");
            if (!string.IsNullOrEmpty(App.Default.VoiceVoxPath))
            {
                if (tp.IsAlive == false)
                {
                    try
                    {
                        //自動起動をトライするが、失敗したって平気さ。知らねぇよ。
                        ProcessStartInfo processStartInfo = new ProcessStartInfo
                        {
                            FileName = App.Default.VoiceVoxPath,
                            WindowStyle = ProcessWindowStyle.Hidden
                        };
                        Process.Start(processStartInfo);
                        await Task.Delay(3000);
                    }
                    catch
                    {
                        richTextBox1.Text += "VOICEVOXの自動起動に失敗しました。";
                        SpeechSynthesizer synth = new SpeechSynthesizer();
                        synth.SelectVoice("Microsoft Haruka Desktop");
                        synth.Speak($"エラーが発生しています。VOICEVOXの自動起動に失敗しました。");
                        Application.Exit();
                    }
                }

                string url = App.Default.VoiceVoxAddress;
                if (url.Substring(url.Length - 1, 1) != "/")
                {
                    url += "/";
                }
                url += "speakers";
                var client = new ServiceHttpClient(url,ServiceHttpClient.RequestType.none);
                var ret = client.Get();
                if (ret != null)
                {
                    //Jsonのデシリアライズ。StyleIdの一覧を作る。
                    List<SpeakerFromAPI> myDeserializedClass = JsonConvert.DeserializeObject<List<SpeakerFromAPI>>(ret.ToString());

                    foreach (SpeakerFromAPI speaker in myDeserializedClass)
                    {
                        foreach (StyleFromAPI st in speaker.styles)
                        {
                            Speaker addLine = new Speaker
                            {
                                StyleId = st.id
                            };
                            StyleDef.Add(addLine);
                        }
                    }
                }
            }

            _ = CheckProcess("SYNCROOM", this.statusStrip1, this.richTextBox1);
        }

        /// <summary>
        /// APIをどついてツイキャスのコメントをチェックする。
        /// </summary>
        static void CheckTwitCasting(ref string LastTwitCastingComment, ref string NowTwitCastingComment)
        {

            if (App.Default.useTwitcasting == false) 
            {
                return;
            }

            if (System.String.IsNullOrEmpty(App.Default.AccessToken))
            {
                return;
            }

            if (System.String.IsNullOrEmpty(App.Default.twitcastUserName))
            {
                return;
            }

            if (System.String.IsNullOrEmpty(App.Default.twitCastingBaseAddress))
            {
                return;
            }

            string baseUrl = App.Default.twitCastingBaseAddress;
                
            if (baseUrl.Substring(baseUrl.Length - 1, 1) != "/")
            {
                baseUrl += "/";
            }

            // User情報の取得
            string url = baseUrl + $"users/{App.Default.twitcastUserName}";
            var client = new ServiceHttpClient(url,ServiceHttpClient.RequestType.twitCasting);
            var ret = client.Get();
            TwitCastingUserInfo userJson = null;
            string lastMovieId="";

            if (ret != null)
            {
                //Jsonのデシリアライズ。LastMovieIdの取得
                userJson = JsonConvert.DeserializeObject<TwitCastingUserInfo>(ret.ToString());
                lastMovieId = userJson.UserInfo.Last_movie_id;
            }

            if (userJson == null)
            {
                return;
            }

            if (System.String.IsNullOrEmpty(lastMovieId))
            {
                return;
            }

            // Movieが生きてるか死んでるか。
#if DEBUG == false
            if (userJson.UserInfo.Is_live == false)
            {
                return;
            }
#endif               
            // 最新配信の取得
            url = baseUrl + $"movies/{lastMovieId}/comments?&limit=3";
            client = new ServiceHttpClient(url, ServiceHttpClient.RequestType.twitCasting);
            ret = client.Get();
            TwitCastingCommentRoot commentsJson = null;

            string LastTwitCastingName = "";
            if (ret != null)
            {
                //Jsonのデシリアライズ。LastMovieIdの取得
                commentsJson = JsonConvert.DeserializeObject<TwitCastingCommentRoot>(ret.ToString());
                NowTwitCastingComment = commentsJson.Comments[0].Message;
            }

            if (App.Default.readName)
            {
                LastTwitCastingName = commentsJson.Comments[0].FromUser.Name;
                NowTwitCastingComment = LastTwitCastingName + " " + NowTwitCastingComment;
            }
            NowTwitCastingComment = LastTwitCastingName + ":" + NowTwitCastingComment;

            if (System.String.IsNullOrEmpty(NowTwitCastingComment))
            {   
                return;
            }

            DateTime lastDt = DateTime.Now;
            DateTime newDt = DateTime.Now;
            CommentText CommentObj = new CommentText(NowTwitCastingComment)
            {
                LastCommentTime = lastDt,
                NowCommentTime = newDt
            };

            //リンクの場合
            if (CommentObj.IsLink)
            {
                CommentObj.Comment = "リンクが張られました。";
                CommentObj.Lang = 0;
                if (File.Exists(App.Default.linkWaveFilePath))
                {
                    SoundPlayer player = new SoundPlayer(App.Default.linkWaveFilePath);
                    //非同期再生する
                    player.Play();
                    LastTwitCastingComment = NowTwitCastingComment;
                    return;
                }
            }
            //空行でない & 連続同一コメントでないこと。
            if (CommentObj.IsBlank == false && LastTwitCastingComment != NowTwitCastingComment)
            {
                //音声用の別処理をぶっ込む。
                SpeechSynthe(CommentObj);
            }

            LastTwitCastingComment = NowTwitCastingComment;
        }


        static async Task CheckProcess(string ProcessName, StatusStrip ststp, RichTextBox logView)
        {
            /*
             * 別タスクに、フォームのコントロールを渡して内容を変更するのは、お作法的にはダメなんだろうなぁ…と思いつつ。
             */

            //初期化。前回取得のログ全部と、最後のコメントの保管場所
            string oldLog = "";
            string lastComment = "";
            DateTime lastDt = DateTime.Now;
            DateTime newDt;

            string LastTwitCastingComment = "";
            string NowTwitCastingComment = "";

            while (true)
            {
                TargetProcess targetProc = new TargetProcess(ProcessName);

                string toolMessage;

                if (targetProc.IsAlive)
                {
                    toolMessage = "は起動されています。";
                    try
                    {
                        //AutomationElement synroomElement = ui.GetMainFrameElement(targetProc.Proc);

                        IntPtr chatwHnd = ui.FindHWndByCaptionAndProcessID("チャット", targetProc.Id);

                        if (chatwHnd != IntPtr.Zero)
                        {
                            ststp.Items[1].Text = "チャットログ監視中。";
                            AutomationElement chatWindow = AutomationElement.FromHandle(chatwHnd);

                            AutomationElement chatLog = ui.GetEditElement(chatWindow, "チャットログ");
                            ValuePattern vp = ui.GetValuePattern(chatLog);

                            if (vp == null) { continue; }

                            Match match;
                            match = Regex.Match(chatLog.Current.Name, "^チャットログ");
                            if (match.Success)
                            {
                                //チャットログに変化があった場合。
                                string newComment = "";
                                newDt = DateTime.Now;

                                if (oldLog != vp.Current.Value)
                                {
                                    oldLog = vp.Current.Value;
                                    logView.Text = vp.Current.Value;

                                    //改行で区切って、一番最後を最新コメントとする。
                                    string[] ary = vp.Current.Value.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

                                    newComment = ary[ary.Count() - 1];

                                    CommentText CommentObj = new CommentText(newComment)
                                    {
                                        LastCommentTime = lastDt,
                                        NowCommentTime = newDt
                                    };

                                    if (App.Default.canSpeech)
                                    {
                                        //リンクの場合
                                        if (CommentObj.IsLink)
                                        {
                                            CommentObj.Comment = "リンクが張られました。";
                                            CommentObj.Lang = 0;
                                            if (File.Exists(App.Default.linkWaveFilePath))
                                            {
                                                SoundPlayer player = new SoundPlayer(App.Default.linkWaveFilePath);
                                                //非同期再生する
                                                player.Play();
                                                lastComment = newComment;
                                                continue;
                                            }
                                        }

                                        //空行でない & 連続同一コメントでないこと。
                                        if (CommentObj.IsBlank == false && lastComment != newComment)
                                        {
                                            //音声用の別処理をぶっ込む。
                                            SpeechSynthe(CommentObj);
                                        }
                                        else
                                        {
                                            // テストモード（音声ファイルは出力しない）で空打ち（エンジンには合成させてる）すると
                                            // 多少は速くなるかなぁと思ったが、そんなに変わらん気もする。
                                            // ここが有効だと、監視中ずーっとエンジン動かしてることにはなるんだよねぇ。
                                            // 一回リリースしてみるか
                                            
                                            CommentObj = new CommentText("空打ち")
                                            {
                                                LastCommentTime = DateTime.Now,
                                                NowCommentTime = DateTime.Now
                                            };
                                            SpeechSynthe(CommentObj, true);
                                            
                                        }
                                    }

                                    lastComment = newComment;
                                    lastDt = newDt;
                                }
                            }
                        }
                        else
                        {                            
                            ststp.Items[1].Text = "チャットログ待機中…";
                        }
                    }
                    catch (ArgumentException agex) { }
                    //hwnd が見つからなかった系のエラーは握りつぶす系。
                    //SyncRoomの起動待ち状態で、起動直後で転ける様子。
                    catch (Exception ex)
                    {
                        string errMsg = $"\r\nエラーが発生しています。{ex.Message}";
                        logView.Text += errMsg;
                        SpeechSynthesizer synth = new SpeechSynthesizer();
                        synth.SelectVoice("Microsoft Haruka Desktop");
                        synth.SpeakAsync(errMsg);
                        await Task.Delay(3000);
                    }
                }
                else
                {
                    toolMessage = "は起動されていません。";
                    oldLog = "";
                    lastComment = "";
                }
                ststp.Items[0].Text = ProcessName + toolMessage;

                CheckTwitCasting(ref LastTwitCastingComment, ref NowTwitCastingComment);

                await Task.Delay((int)App.Default.waitTiming);
            }
        }

        private class CommentText
        {

            static readonly int[] randTable = new int[] { 0, 8, 10, 14, 20, 21, 12, 13, 11, 3, 29, 30, 23, 27 };
            static readonly string blankLineUserName = "改行コピペ野郎";

            public bool IsBlank;
            public bool IsLink;
            public bool ChimeFlg = true;
            public bool SpeechFlg = true;
            public int Lang;
            public int StyleId = 2;
            public string UserName { get; set; }
            public string Comment { get; set; }
            public DateTime LastCommentTime;
            public DateTime NowCommentTime;

            public CommentText(string newComment)
            {
                //末尾がコロンかどうか。空行チェック。
                IsBlank = (newComment.Substring(newComment.Length - 1, 1) == ":");

                //リンクかどうかのチェック
                Match match;
                match = Regex.Match(newComment, "https?://");
                IsLink = (match.Success);

                //英数のみかのチェックというか、指定のワードが入ってるかどうか（主に日本語）
                match = Regex.Match(newComment, "[ぁ-んァ-ヶｱ-ﾝﾞﾟ一-龠！-／：-＠［-｀｛-～、-〜”’・]");
                if (match.Success)
                {
                    Lang = 0;
                }
                else
                {
                    Lang = 1;
                }

                //ユーザ名の取得と、コロンを除いたコメント部分の取得
                string[] ary = newComment.Split(new string[] { ":" }, StringSplitOptions.None);
                if (ary.Length < 2)
                {
                    UserName = blankLineUserName;
                    Comment = newComment;
                }
                else
                {
                    UserName = ary[ary.Length - (int)CommentDivider.UserName];
                    Comment = ary[ary.Length - (int)CommentDivider.Comment];
                }

                if (UserName == blankLineUserName)
                {
                    //改行コピペマンに対しては、ユーザ周りの処理をしなくていいんじゃないかなと。コマンド系の処理も。
                    return;
                }

                //ランダム音声割り当て用。ここ、コメントしたら全員デフォでしゃべる。
                Random rnd = new Random();
                StyleId = rnd.Next(randTable.Length);

                bool existsFlg = UserTable.Exists(x => x.UserName == UserName);

                if (existsFlg)
                {
                    //UserTableから、StyleIdその他の取り出し。
                    foreach (var item in UserTable.Where(x => x.UserName == UserName))
                    {
                        StyleId = item.StyleId;
                        ChimeFlg = item.ChimeFlg;
                        SpeechFlg = item.SpeechFlg;
                        break;
                    }
                }
                else
                {
                    UpdateUserOption(existsFlg, UserName, StyleId, ChimeFlg, SpeechFlg);
                }

                //行頭のコマンド有無のチェック。スタイル指定。
                match = Regex.Match(Comment, @"^\/\d{1,2}");
                if (match.Success)
                {
                    Comment = Comment.Replace(match.ToString(), "");
                    //[数値]な形式の数値ではある。桁指定したので、[0]～[99]まで。
                    match = Regex.Match(match.ToString(), @"\d{1,2}");
                    if (match.Success)
                    {
                        //数値は取れたので範囲チェック。StyleIdの一覧と比較。
                        if (StyleDef.Exists(x => x.StyleId == int.Parse(match.ToString())))
                        {
                            StyleId = int.Parse(match.ToString());

                            //[]で指定された数値が、スタイル一覧と合致した場合は、UserTableになければ追加、あれば更新。
                            UpdateUserOption(existsFlg, UserName, StyleId, ChimeFlg, SpeechFlg);
                        }
                    }
                }

                //行頭コマンドチェック。/s はスピーチのトグル
                match = Regex.Match(Comment, @"^/s", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    Comment = Comment.Replace(match.ToString(), "");
                    UpdateUserOption(existsFlg, UserName, StyleId, ChimeFlg, !SpeechFlg);
                }

                //行頭コマンドチェック。/c はスピーチのトグル
                match = Regex.Match(Comment, @"^/c", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    Comment = Comment.Replace(match.ToString(), "");
                    UpdateUserOption(existsFlg, UserName, StyleId, !ChimeFlg, SpeechFlg);
                }

                //UserTableから、StyleIdその他の取り出し。
                foreach (var item in UserTable.Where(x => x.UserName == UserName))
                {
                    StyleId = item.StyleId;
                    ChimeFlg = item.ChimeFlg;
                    SpeechFlg = item.SpeechFlg;
                    break;
                }

                //8888対応
                match = Regex.Match(Comment, @"(８|8){2,}", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    Comment = Comment.Replace(match.ToString(), "、パチパチパチ");
                    Lang = 0;
                }

                //8888対応
                match = Regex.Match(Comment, @"(８|8){1,}", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    Comment = Comment.Replace(match.ToString(), "、パチ");
                    Lang = 0;
                }

                //ｗｗｗ対応
                match = Regex.Match(Comment, @"(ｗ|w){2,}", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    Comment = Comment.Replace(match.ToString(), "、ふふっ");
                    Lang = 0;
                }

                //行末のｗｗｗ対応
                match = Regex.Match(Comment, @"(ｗ|w){1,}$", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    Comment = Comment.Replace(match.ToString(), "、ふふっ");
                    Lang = 0;
                }

                //文字数制限
                if (Comment.Length > commentLimit)
                {
                    string[] cutText = { "、以下略。", ", Omitted below" };
                    Comment = Comment.Substring(0, commentLimit - 1);
                    Comment += cutText[Lang];
                }

            }
        }

        private static void UpdateUserOption(bool existsFlg, string UserName, int StyleId, bool ChatFlg, bool SpeechFlg)
        {
            if (existsFlg)
            {
                foreach (var item in UserTable.Where(x => x.UserName == UserName))
                {
                    item.StyleId = StyleId;
                    item.UserName = UserName;
                    item.ChimeFlg = ChatFlg;
                    item.SpeechFlg = SpeechFlg;
                }
            }
            else
            {
                Speaker addLine = new Speaker
                {
                    StyleId = StyleId,
                    UserName = UserName,
                    ChimeFlg = ChatFlg,
                    SpeechFlg = SpeechFlg
                };
                UserTable.Add(addLine);
            }
        }

        private static async void SpeechSynthe(CommentText commentObj, bool testMode = false)
        {
            //多分引っかからないとは思いつつ。
            if (string.IsNullOrEmpty(commentObj.Comment))
            {
                return;
            }

            //メッセージ（情報）を鳴らす
            TimeSpan commentDiff = commentObj.NowCommentTime - commentObj.LastCommentTime;

            if (commentObj.ChimeFlg)
            {
                if (commentDiff.TotalSeconds > 5)
                {
                    SystemSounds.Asterisk.Play();
                    await Task.Delay(100);
                }
            }

            if (commentObj.SpeechFlg == false)
            {
                return;
            }

            if (App.Default.UseVoiceVox == true && commentObj.Lang == 0)
            {
                SpeechVOICEVOX(commentObj, testMode);
                return;
            }

            SpeechSynthesizer synth = new SpeechSynthesizer
            {
                Rate = -1
            };

            //lang == 0 日本語でしゃべる。VOICEVOXを使うかどうかで処理分岐。
            if (commentObj.Lang == 0)
            {
                synth.SelectVoice("Microsoft Haruka Desktop");
            }
            else
            {
                synth.SelectVoice("Microsoft Zira Desktop");
            }

            synth.SpeakAsync(commentObj.Comment);
        }

        private static void SpeechVOICEVOX(CommentText commentObj, bool testMode = false)
        {
            string baseUrl = App.Default.VoiceVoxAddress;
            string url;

            if (string.IsNullOrEmpty(baseUrl))
            {
                /*
                Debug.WriteLine("URLが入ってない");
                SpeechSynthesizer synth = new SpeechSynthesizer();
                synth.SelectVoice("Microsoft Haruka Desktop");
                synth.SpeakAsync($"エラーが発生しています。ベースURLが入っていません。");
                return;
                */
                //たまに消えるよね、君。
                baseUrl = "http://localhost:50021";
            }

            if (baseUrl.Substring(baseUrl.Length - 1, 1) != "/")
            {
                baseUrl += "/";
            }

            url = baseUrl + $"audio_query?text='{commentObj.Comment}'&speaker={commentObj.StyleId}";

            var client = new ServiceHttpClient(url,ServiceHttpClient.RequestType.none);
            String QueryResponce = "";
            var ret = client.Post(ref QueryResponce, "");

            if (ret == null)
            {
                return;
            }

            if (testMode)
            {
                return;
            }

            if (ret.StatusCode.Equals(HttpStatusCode.OK))
            {
                url = baseUrl + $"synthesis?speaker={commentObj.StyleId}";
                client = new ServiceHttpClient(url,ServiceHttpClient.RequestType.none);
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
                App.Default.cutLength = apConf.cutLength;
                App.Default.VoiceVoxAddress = apConf.voiceVoxAddress;
                App.Default.VoiceVoxPath = apConf.voiceVoxPath;
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
            App.Default.useTwitcasting = MenuEnableTwitcasting.Checked;
            App.Default.Save();
        }

        private void RichTextBox1_TextChanged(object sender, EventArgs e)
        {
            //カレット位置を末尾に移動
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            //カレット位置までスクロール
            richTextBox1.ScrollToCaret();
        }

        private void Menu_EnebleSpeech_Click(object sender, EventArgs e)
        {
            App.Default.canSpeech = MenuEnebleSpeech.Checked;
            App.Default.Save();
        }

        private void Menu_VoiceVox_Click(object sender, EventArgs e)
        {
            App.Default.UseVoiceVox = MenuUseVoiceVox.Checked;
            App.Default.Save();
        }

        private void Menu_Help_Click(object sender, EventArgs e)
        {
            Help help = new Help();
            help.Width = (int)Math.Floor(this.Width * 0.9);
            help.Height = (int)Math.Floor(this.Height * 0.9);

            help.ShowDialog();
        }

        private void MenuSettingTwitcasting_Click(object sender, EventArgs e)
        {
            Twitcasting twitCasting = new Twitcasting();
            if (twitCasting.ShowDialog() == DialogResult.OK)
            {
                App.Default.AccessToken = twitCasting.AccessToken;
                App.Default.twitcastUserName = twitCasting.TwitcastUserName;
                App.Default.readName = twitCasting.ReadName;
                App.Default.Save();
            }
        }

        private void MenuEnableTwitcasting_Click(object sender, EventArgs e)
        {
            App.Default.useTwitcasting = MenuEnableTwitcasting.Checked;
            App.Default.Save();
        }

        private void MenuTwitcasting_Click(object sender, EventArgs e)
        {

        }
    }
}
