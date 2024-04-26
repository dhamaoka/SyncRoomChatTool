using NAudio.Wave;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Reflection;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Forms;
using DiffMatchPatch;

namespace SyncRoomChatTool
{
    public partial class Form1 : Form
    {
        enum CommentDivider : int
        {
            Comment = 1,
            UserName = 2,
            LinkedUserName = 3
        }

        //スレッド内でのコントロール制御用Delegete達
        delegate void DelegeteUpdateStatusStrip(int idx, string text);
        delegate void DelegeteUpdateRichText(string text);
        delegate bool DelegeteIsLogEmpty();

        /// <summary>
        /// 別スレッドからRichTextBoxが空かどうかの判定用のはずだが。ダメな気もする。
        /// </summary>
        /// <returns></returns>
        private bool IsLogEmpty()
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new DelegeteIsLogEmpty(this.IsLogEmpty));
                }
                else
                {
                    if (string.IsNullOrEmpty(richTextBox1.Text)) { return true; }
                }
            }
            catch
            {
                Application.Exit();
            };
            return false;
        }

        /// <summary>
        /// 別スレッドからRichTextBoxを更新する。
        /// </summary>
        /// <param name="text"></param>
        private void UpdateRichText(string text)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new DelegeteUpdateRichText(this.UpdateRichText), new object[] { text });
                }
                else
                {
                    richTextBox1.AppendText(text);
                }
            }
            catch
            {
                Application.Exit();
            };
        }

        /// <summary>
        /// 別スレッドからステータスストリップを更新する。
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="text"></param>
        private void UpdateStatusStrip(int idx, string text)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new DelegeteUpdateStatusStrip(this.UpdateStatusStrip), new object[] { idx, text });
                }
                else
                {
                    statusStrip1.Items[idx].Text = text;
                }

            }
            catch
            {
                Application.Exit();
            };
        }

        private static string LastTwitCastingComment = "";
        private static string NowTwitCastingComment = "";
        private static int commentCounter = 0;
        private static string LastURL = "";

        private static User TwiCasUser = new User{ };
        private AutoCompleteStringCollection autoCompList;

        public Form1()
        {
            InitializeComponent();
        }

        static readonly List<Speaker> VoiceLists = new List<Speaker> { };

        private static readonly UIAutomationLib ui = new UIAutomationLib { };
        private static readonly string voiceVoxDefaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs\\VOICEVOX\\vv-engine\\run.exe");
        private static readonly string voiceVoxDefaultOldPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs\\VOICEVOX\\run.exe");

        static readonly List<Speaker> UserTable = new List<Speaker> { };
        static readonly List<Speaker> StyleDef = new List<Speaker> { };

        static readonly BlockingCollection<CommentObject> CommentQue = new BlockingCollection<CommentObject>();

        private class CommentObject
        {
            static readonly int[] RandTable = new int[] { 0, 1, 2, 3, 6, 7, 8, 9, 10, 14, 16, 20, 23, 29 };
            static readonly string BlankLineUserName = "改行コピペ野郎";

            public bool IsLink;
            public bool ChimeFlg = false;
            public bool SpeechFlg = true;
            public int Lang = 0;
            public int StyleId = 2;
            public string UserName { get; set; }
            public string Comment { get; set; }
            public string RawComment { get; set; }
            public string UriString { get; set; }
            public DateTime LastCommentTime;
            public DateTime NowCommentTime;
            public bool CanSpeech = false;
            public double SpeedScale = 1;

            public CommentObject(string newComment, string lastComment, bool appCanSpeech)
            {
                //末尾がコロンかどうか。空行チェック。
                if (newComment.Substring(newComment.Length - 1, 1) == ":")
                {
                    return;
                }

                //ユーザ名の取得と、コロンを除いたコメント部分の取得
                string[] ary = newComment.Split(new string[] { ":" }, StringSplitOptions.None);
                if (ary.Length < 2)
                {
                    UserName = BlankLineUserName;
                    Comment = newComment;
                }
                else
                {
                    UserName = ary[ary.Length - (int)CommentDivider.UserName];
                    Comment = ary[ary.Length - (int)CommentDivider.Comment];
                    RawComment = Comment;
                }

                UserName.Trim();
                if (UserName == BlankLineUserName)
                {
                    //改行コピペマンに対しては、ユーザ周りの処理をしなくていいんじゃないかなと。コマンド系の処理も。
                    return;
                }

                //リンクかどうかのチェック
                Match match;
                match = Regex.Match(newComment, "https?://");
                IsLink = (match.Success);
                if (IsLink)
                {
                    //リンクが貼られたときはここで発声させてる。
                    UserName = ary[ary.Length - (int)CommentDivider.LinkedUserName];
                    Comment = "リンクが張られました。";
                    Lang = 0;
                    if (appCanSpeech)
                    {
                        if (File.Exists(App.Default.linkWaveFilePath))
                        {
                            /*
                            //固定ファイルが設定されている場合は直再生。
                            SoundPlayer player = new SoundPlayer(App.Default.linkWaveFilePath);
                            //再生する。
                            if (App.Default.PlayAsync) { 
                                //非同期
                                player.Play(); 
                            }
                            else
                            {
                                //同期
                                player.PlaySync();
                            }
                            */

                            var waveReader = new WaveFileReader(App.Default.linkWaveFilePath);
                            var waveOut = new WaveOut
                            {
                                DeviceNumber = App.Default.OutputDevice,
                            };
                            waveOut.Init(waveReader);
                            waveOut.Play();

                            if (App.Default.PlayAsync)
                            {
                                Thread.Sleep(2000);
                                //非同期
                                //player.Play();
                            }
                            else
                            {
                                //SoundPlayer player = new SoundPlayer(wavFile);
                                //同期
                                //player.PlaySync();
                                while (waveOut.PlaybackState == PlaybackState.Playing)
                                {
                                    Thread.Sleep(50);
                                }
                            }

                        }
                        else
                        {
                            CommentQue.TryAdd(this);
                        }
                    }

                    //自動リンクオープン部分。
                    UriString = newComment.Substring(match.Index);
                    Uri u = new Uri(UriString);

                    if (App.Default.OpenLink)
                    {
                        if (UriString != LastURL)
                            {
                                if (u.IsAbsoluteUri)
                            {
                                Process.Start(UriString);
                            }
                        }
                    }

                    LastURL = UriString;
                    return;
                }

                if (appCanSpeech == false) { return; }

                //絵文字っぽいのが入っているかどうかのチェック。半角スペースに置換
                var newCommentChar = Comment.ToCharArray();
                for (int i = 0; i < newCommentChar.Length; i++)
                {
                    switch (char.GetUnicodeCategory(newCommentChar[i]))
                    {
                        case System.Globalization.UnicodeCategory.Surrogate:
                            newCommentChar[i] = Convert.ToChar(" ");
                            break;
                        case System.Globalization.UnicodeCategory.OtherSymbol:
                            newCommentChar[i] = Convert.ToChar(" ");
                            break;
                        case System.Globalization.UnicodeCategory.PrivateUse:
                            newCommentChar[i] = Convert.ToChar(" ");
                            break;
                    }
                }

                Comment = new string(newCommentChar);
                Comment.Trim();

                //ωのチェック。これうざいので。
                match = Regex.Match(Comment, @"[ω]");
                if (match.Success)
                {
                    Comment.Replace("ω", "");
                }

                if (string.IsNullOrEmpty(Comment)) { return; }

                //英数のみかのチェックというか、指定のワードが入ってるかどうか（主に日本語）
                match = Regex.Match(Comment, "[ぁ-んァ-ヶｱ-ﾝﾞﾟ一-龠！-／：-＠［-｀｛-～、-〜”’・]");
                if (match.Success == false)
                {
                    Lang = 1;
                }

                //ランダム音声割り当て用。ここ、コメントしたら全員デフォでしゃべる。
                Random rnd = new Random{ };
                StyleId = RandTable[rnd.Next(RandTable.Length)];

                bool existsFlg = UserTable.Exists(x => x.UserName == UserName);

                if (existsFlg)
                {
                    //UserTableから、StyleIdその他の取り出し。
                    foreach (var item in UserTable.Where(x => x.UserName == UserName))
                    {
                        StyleId = item.StyleId;
                        ChimeFlg = item.ChimeFlg;
                        SpeechFlg = item.SpeechFlg;
                        SpeedScale = item.SpeedScale;
                        break;
                    }
                }
                else
                {
                    UpdateUserOption(existsFlg, UserName, StyleId, ChimeFlg, SpeechFlg, SpeedScale);
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
                            UpdateUserOption(existsFlg, UserName, StyleId, ChimeFlg, SpeechFlg, SpeedScale);
                        }
                    }
                }

                //行頭のコマンド有無のチェック。スピード指定。
                match = Regex.Match(Comment, @"^/p", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    //まずは/pで始まってるか。見つかったらそれはコメントから除去
                    Comment = Comment.Replace(match.ToString(), "");
                    match = Regex.Match(Comment, @"^[[0-9]+[.]?[0-9]{1,1}|[0-9]+]");
                    if (match.Success)
                    {
                        //次に数字があるか。
                        Comment = Comment.Replace(match.ToString(), "");
                        SpeedScale = Convert.ToDouble(match.ToString());
                        if (SpeedScale > 1.8)
                        {
                            SpeedScale = 1.8;
                        }
                        if (SpeedScale < 0.4)
                        {
                            SpeedScale = 0.4;
                        }
                        UpdateUserOption(existsFlg, UserName, StyleId, ChimeFlg, SpeechFlg, SpeedScale);
                    }
                }

                //行頭コマンドチェック。/s はスピーチのトグル
                match = Regex.Match(Comment, @"^/s", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    Comment = Comment.Replace(match.ToString(), "");
                    UpdateUserOption(existsFlg, UserName, StyleId, ChimeFlg, !SpeechFlg, SpeedScale);
                }

                //行頭コマンドチェック。/c はチャイムのトグル
                match = Regex.Match(Comment, @"^/c", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    Comment = Comment.Replace(match.ToString(), "");
                    UpdateUserOption(existsFlg, UserName, StyleId, !ChimeFlg, SpeechFlg, SpeedScale);
                }

                //UserTableから、StyleIdその他の取り出し。
                foreach (var item in UserTable.Where(x => x.UserName == UserName))
                {
                    StyleId = item.StyleId;
                    ChimeFlg = item.ChimeFlg;
                    SpeechFlg = item.SpeechFlg;
                    break;
                }

                //名前にツイキャスユーザが入っている場合。
                if (Regex.Match(newComment, "ツイキャスユーザ").Success)
                {
                    StyleId = 8;
                }

                //8888対応
                match = Regex.Match(Comment, @"(８|8){2,}", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    Lang = 0;
                    Comment = Comment.Replace(match.ToString(), "、パチパチパチ");
                }

                //8888対応
                match = Regex.Match(Comment, @"(８|8){1,}", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    Lang = 0;
                    Comment = Comment.Replace(match.ToString(), "、パチ");
                }

                //ｗｗｗ対応
                match = Regex.Match(Comment, @"(ｗ|w){2,}", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    Lang = 0;
                    Comment = Comment.Replace(match.ToString(), "、ふふっ");
                }

                //行末のｗｗｗ対応
                match = Regex.Match(Comment, @"(ｗ|w){1,}$", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    Lang = 0;
                    Comment = Comment.Replace(match.ToString(), "、ふふっ");
                }

                //文字数制限
                if (Comment.Length > (int)App.Default.cutLength)
                {
                    string[] cutText = { "、以下略。", ", Omitted below" };
                    Comment = Comment.Substring(0, (int)(App.Default.cutLength - 1));
                    Comment += cutText[Lang];
                }

                //前回コメントとの比較
                if (lastComment == newComment)
                {
                    return;
                }
                CanSpeech = true;
            }
        }

        /// <summary>
        /// PCのオーディオ出力一覧を取得（とコンボボックスに追加）
        /// </summary>
        /// <returns></returns>
        private List<string> GetDevices()
        {
            List<string> deviceList = new List<string>();        
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                var capabilities = WaveOut.GetCapabilities(i);
                deviceList.Add(capabilities.ProductName);
#if DEBUG
                Debug.WriteLine($"{i}:{capabilities.ProductName}");
#endif
                toolStripComboBox1.Items.Add(capabilities.ProductName);
            }

            /*
            //ソートダメっぽい。内部とインデックスが違うので。
            var list = deviceList.ToArray().AsEnumerable().OrderBy(x => x);

            foreach (var device in list)
            {
                toolStripComboBox1.Items.Add(device.ToString());
            } 
            */

            return deviceList;
        }

        private static void UpdateUserOption(bool existsFlg, string UserName, int StyleId, bool ChatFlg, bool SpeechFlg, double SpeedScale)
        {
            if (existsFlg)
            {
                foreach (var item in UserTable.Where(x => x.UserName == UserName))
                {
                    item.StyleId = StyleId;
                    item.UserName = UserName;
                    item.ChimeFlg = ChatFlg;
                    item.SpeechFlg = SpeechFlg;
                    item.SpeedScale = SpeedScale;
                }
            }
            else
            {
                Speaker addLine = new Speaker
                {
                    StyleId = StyleId,
                    UserName = UserName,
                    ChimeFlg = ChatFlg,
                    SpeechFlg = SpeechFlg,
                    SpeedScale = SpeedScale
                };
                UserTable.Add(addLine);
            }
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            //前のバージョンのプロパティを引き継ぐぜ。
            App.Default.Upgrade();

            //アセンブリーからバージョン取得
            var assembly = Assembly.GetExecutingAssembly();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

            //バージョンの表示
            this.Text += $" {fileVersionInfo.ProductVersion}";

            //オートコンプリート機能
            autoCompList = new AutoCompleteStringCollection();
            textBox1.AutoCompleteCustomSource = autoCompList;

            //各種オプションの取得
            MenuEnableTwitcasting.Checked = App.Default.useTwitcasting;
            MenuEnebleSpeech.Checked = App.Default.canSpeech;
            MenuUseVoiceVox.Checked = App.Default.UseVoiceVox;
            MenuWindowTopMost.Checked = App.Default.WindowTopMost;
            MenuOpenLink.Checked = App.Default.OpenLink;
            this.TopMost = MenuWindowTopMost.Checked;
            richTextBox1.LanguageOption = RichTextBoxLanguageOptions.UIFonts;
            richTextBox1.Font = App.Default.logFont;
            richTextBox1.ZoomFactor = App.Default.zoomFacter;
            if (App.Default.waitTiming < 100)
            {
                App.Default.waitTiming = 100;
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
                    //VOICEVOX 0.16 以降のバージョンパス（vv-engine）
                    App.Default.VoiceVoxPath = voiceVoxDefaultPath;
                }
                else
                {
                    //パス設定なし＝初回＆VOICEVOX 0.16 未満のバージョン（旧パス）
                    App.Default.VoiceVoxPath = voiceVoxDefaultOldPath;
                }
            }
            else
            {
                if (!File.Exists(App.Default.VoiceVoxPath))
                {
                    //VOICEVOXデフォルトパスにRun.exeが居る＝インストールされているとみなし、
                    if (File.Exists(voiceVoxDefaultPath))
                    {
                        //設定に保存する＝VOICEVOXが使えると見なす。
                        //VOICEVOX 0.16 以降のバージョンパス（vv-engine）
                        App.Default.VoiceVoxPath = voiceVoxDefaultPath;
                    }
                    else
                    {
                        //パス設定なし＝初回＆VOICEVOX 0.16 未満のバージョン（旧パス）
                        App.Default.VoiceVoxPath = voiceVoxDefaultOldPath;
                    }
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
                App.Default.VoiceVoxAddress = "http://127.0.0.1:50021";
            }

            App.Default.Save();

            //VOICEVOXエンジンの起動チェック
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
                        SpeechSynthesizer synth = new SpeechSynthesizer{ };
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
                var client = new ServiceHttpClient(url, ServiceHttpClient.RequestType.none);
                var ret = client.Get();
                if (ret != null)
                {
                    //Jsonのデシリアライズ。VOICEVOXのStyleIdの一覧を作る。
                    List<SpeakerFromAPI> VoiceVoxSpeakers = JsonConvert.DeserializeObject<List<SpeakerFromAPI>>(ret.ToString());

                    foreach (SpeakerFromAPI speaker in VoiceVoxSpeakers)
                    {
                        foreach (StyleFromAPI st in speaker.Styles)
                        {
                            Speaker addLine = new Speaker
                            {
                                StyleId = st.Id
                            };

                            //ホントはSyncRoomのユーザ用のClassだけど、Voiceの一覧にも流用
                            //ホントは自分のIDとボイス名だけでもいい気がするんだけど、そのマッチは面倒だったので。
                            //チャットが入る度に、その人の名前と割り当てられたボイスをステータスバーに表示。
                            //速いと流れるね。最後の人のは見れるけど。参考程度の情報。声変えたい人が居たら教えてあげられるぐらいの。
                            Speaker addVoice = new Speaker
                            {
                                UserName = $"{speaker.Name}({st.Name})",
                                StyleId = addLine.StyleId
                            };

                            VoiceLists.Add(addVoice);
                            StyleDef.Add(addLine);
                        }
                    }

                    VoiceLists.Sort((a,b)=> a.StyleId - b.StyleId);
                    foreach (Speaker st in VoiceLists)
                    {
                        // 候補リストに項目を追加（初期設定）
                        autoCompList.Add($"/{st.StyleId} {st.UserName} にボイス変更");
                    }
                    autoCompList.Add("/p0.4 最小スピード");
                    autoCompList.Add("/p1.0 標準スピード");
                    autoCompList.Add("/p1.8 最大スピード");
                }
            }
            autoCompList.Add("/c チャイムのトグル");
            autoCompList.Add("/s スピーチのトグル");

            GetDevices();

            //メインループを実行。
            _ = TaskGetChat("SYNCROOM");

            //ツイキャス配信の確認。5秒ごとに更新で固定。
            _ = TaskGetUserInfo();

            //ツイキャスコメント取得。1010ミリ秒間隔で取得で固定。
            _ = TaskGetTwicasCommen();

            //キューからの取得と音声合成。キューがなくなるまで合成して発話する。
            _ = TaskTryTakeAndSpeech();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            try
            {
                toolStripComboBox1.SelectedIndex = App.Default.OutputDevice;
            }
            catch (Exception)
            {
            }
            toolStripComboBox1.Select();

            //todo:ロードでやってるのが良くないのか、起動してなんぞ動かんと表示が変わらん問題。Shownに移したが変わらん。
            //何故かチャットウィンドウ開いたら、反応するのよねぇ。
            richTextBox1.Refresh();
            this.Size = App.Default.windowSize;
            this.Refresh();
            textBox1.Focus();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                App.Default.zoomFacter = richTextBox1.ZoomFactor;
                App.Default.logFont = richTextBox1.Font;
                App.Default.windowSize = this.Size;
                App.Default.useTwitcasting = MenuEnableTwitcasting.Checked;
                App.Default.WindowTopMost = MenuWindowTopMost.Checked;
                App.Default.OpenLink = MenuOpenLink.Checked;
                if (toolStripComboBox1.SelectedIndex == -1)
                {
                    App.Default.OutputDevice = 0;
                }
                else
                {
                    App.Default.OutputDevice = toolStripComboBox1.SelectedIndex;
                }
                App.Default.Save();
            }
            catch
            {
                Application.Exit();
            }
        }
        async Task TaskGetChat(string ProcessName)
        {
            //初期化。前回取得のログ全部と、最後のコメントの保管場所
            string oldLog = "";
            string oldComment = "";
            DateTime lastDt = DateTime.Now;
            DateTime newDt;
            UpdateRichText("");
            await Task.Run(async () =>
            {
                while (true)
                {
                    TargetProcess targetProc = new TargetProcess(ProcessName);

                    string toolMessage = "";

                    if (targetProc.IsAlive)
                    {
                        toolMessage = "は起動されています。";
                        try
                        {
                            IntPtr chatwHnd = ui.FindHWndByCaptionAndProcessID("チャット", targetProc.Id);

                            if (chatwHnd != IntPtr.Zero)
                            {
                                UpdateStatusStrip(1, "チャットログ監視中。");

                                AutomationElement chatWindow = AutomationElement.FromHandle(chatwHnd);

                                AutomationElement chatLog = ui.GetEditElement(chatWindow, "チャットログ");
                                ValuePattern vp = ui.GetValuePattern(chatLog);

                                if (vp == null) { continue; }

                                diff_match_patch diffmatch = new diff_match_patch();

                                Match match;
                                match = Regex.Match(chatLog.Current.Name, "^チャットログ");
                                if (match.Success)
                                {
                                    //チャットログに変化があった場合。
                                    List<Diff> diffs = diffmatch.diff_main(oldLog, vp.Current.Value);
                                    diffmatch.diff_cleanupSemantic(diffs);

                                    if ((diffs.Count > 0) && (!string.IsNullOrEmpty(oldLog)) || ((diffs.Count == 0) && string.IsNullOrEmpty(oldLog)))
                                    {
                                        foreach (Diff result in diffs)
                                        {
                                            if (result.operation == Operation.INSERT)
                                            {
                                                string[] lines = result.text.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                                                foreach (string newComment in lines)
                                                {
                                                    newDt = DateTime.Now;
                                                    CommentObject CommentObj = new CommentObject(newComment, oldComment, App.Default.canSpeech)
                                                    {
                                                        LastCommentTime = lastDt,
                                                        NowCommentTime = newDt
                                                    };
                                                    if (string.IsNullOrEmpty(CommentObj.Comment) == false)
                                                    {
                                                        //何かコメントあり。
                                                        if (IsLogEmpty() == false)
                                                        {
                                                            //かつ、リッチテキストボックスが空じゃない＝改行する。
                                                            UpdateRichText(Environment.NewLine);
                                                        }

                                                        string addLine = CommentObj.UserName;
                                                        if (string.IsNullOrEmpty(addLine) == false)
                                                        {
                                                            UpdateRichText(addLine);

                                                            string separator = " ： ";
                                                            if (CommentObj.IsLink)
                                                            {
                                                                //リンクだった時の処理
                                                                UpdateRichText(Environment.NewLine);
                                                                addLine = " \t" + CommentObj.UriString;
                                                            }
                                                            else
                                                            {
                                                                //リンクじゃない。
                                                                //名前はもう出してるので、CommentObject.Commentじゃなく、RawComment を新たに作成。
                                                                addLine = separator + CommentObj.RawComment;
                                                            }

                                                            UpdateRichText(addLine);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        //コメントなし。改行のみ。
                                                        CommentObj.CanSpeech = false;
                                                    }

                                                    if (CommentObj.CanSpeech)
                                                    {
                                                        //キューにぶっ込む。
                                                        CommentQue.TryAdd(CommentObj);
                                                    }

                                                    oldComment = newComment;
                                                    lastDt = newDt;

                                                    bool existsFlg = VoiceLists.Exists(x => x.StyleId == CommentObj.StyleId);
                                                    if (existsFlg)
                                                    {
                                                        //VoiceListsから、StyleIdその他の取り出し。
                                                        foreach (var item in VoiceLists.Where(x => x.StyleId == CommentObj.StyleId))
                                                        {
                                                            UpdateStatusStrip(3, $"名前：{CommentObj.UserName} ボイス：[{item.StyleId}]{item.UserName}");
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    oldLog = vp.Current.Value;
                                }
                            }
                            else
                            {
                                UpdateStatusStrip(1, "チャットログ待機中…");
                            }
                        }
                        catch (ArgumentException agex)
                        {
                            //hwnd が見つからなかった系のエラーは握りつぶす系。
                            //SyncRoomの起動待ち状態で、起動直後で転ける様子。
                            UpdateStatusStrip(1, $"{agex.Message}");
                        }
                        catch (Exception ex)
                        {
                            string errMsg = $"\r\nエラーが発生しています。{ex.Message}";
                            UpdateRichText(errMsg);
                            SpeechSynthesizer synth = new SpeechSynthesizer { };
                            synth.SelectVoice("Microsoft Haruka Desktop");
                            synth.Speak(errMsg);
                            await Task.Delay(3000);
                            Application.Exit();
                        }
                    }
                    else
                    {
                        toolMessage = "は起動されていません。";
                        UpdateStatusStrip(1, "チャットウィンドウなし。");
                        oldLog = "";
                        oldComment = "";
                    }
                    UpdateStatusStrip(0, ProcessName + toolMessage);

                    await Task.Delay((int)App.Default.waitTiming);
                }
            }
            );
        }

        async Task TaskGetTwicasCommen()
        {
            await Task.Run(async ()=> {
                while (true)
                {
                    if (App.Default.useTwitcasting) {
                        GetTwicasComment();
#if DEBUG
                        Debug.Print($"Twicas API Done. {DateTime.Now}");
#endif
                    }

                    //約1秒に1回を強制。ちょっとマージン付けた。ツイキャスの仕様で、
                    //1分間に60以上リクエストするのはダメなので。
                    await Task.Delay(1010);
                }
            });
        }

        /// <summary>
        /// ツイキャスの配信者情報取得
        /// </summary>
        async Task TaskGetUserInfo()
        {
            await Task.Run(async () => {
                while (true)
                {
                    UpdateStatusStrip(2, "");

                    if (App.Default.useTwitcasting)
                    {
                        //ユーザ情報取得のリクエストを投げる
                        GetTwicasUserInfo();

                        if (TwiCasUser != null)
                        {
                            string castingStatus = "未配信";
                            if (TwiCasUser.IsAlive)
                            {
                                castingStatus = "配信中";
                            }
                            UpdateStatusStrip(2, $"ツイキャスID = {TwiCasUser.ScreenId}（{TwiCasUser.Name}）{castingStatus}");
                        }
                        else
                        {
                            UpdateStatusStrip(2, "ツイキャスユーザ情報の取得に失敗しました。");
                        }
                    }

                    //ツイキャスユーザの更新は5秒で固定。
                    await Task.Delay(5000);
                }
            });
        }

        /// <summary>
        /// ツイキャスユーザ情報取得。
        /// </summary>
        private void GetTwicasUserInfo()
        {
            if (App.Default.useTwitcasting == false) { return; }

            if (string.IsNullOrEmpty(App.Default.AccessToken)) { return; }

            if (string.IsNullOrEmpty(App.Default.twitcastUserName)) { return; }

            if (string.IsNullOrEmpty(App.Default.twitCastingBaseAddress)) { return; }

            string baseUrl = App.Default.twitCastingBaseAddress;

            if (baseUrl.Substring(baseUrl.Length - 1, 1) != "/")
            {
                baseUrl += "/";
            }

            // User情報の取得
            string url = baseUrl + $"users/{App.Default.twitcastUserName}";
            var client = new ServiceHttpClient(url, ServiceHttpClient.RequestType.userInfo);
            var ret = client.Get();
            TwitCastingUserInfo userJson = null;

            if (ret != null)
            {
                //Jsonのデシリアライズ。LastMovieIdの取得
                userJson = JsonConvert.DeserializeObject<TwitCastingUserInfo>(ret.ToString());
            }
            TwiCasUser = null;
            if (userJson != null)
            {
                TwiCasUser = userJson.UserInfo;
            }

            return;
        }

        /// <summary>
        /// APIをどついてツイキャスのコメントをチェックする。
        /// </summary>
        private void GetTwicasComment()
        {
            //アプリ設定でツイキャス連携なしなら、3秒後ループする。
            //ツイキャス連携のスタートが少々遅れたとしても問題なかろう。
            if (App.Default.useTwitcasting == false)
            {
                Task.Delay(3000);
                return;
            }

            if (string.IsNullOrEmpty(App.Default.AccessToken)) { return; }
            if (string.IsNullOrEmpty(App.Default.twitcastUserName)) { return; }
            if (string.IsNullOrEmpty(App.Default.twitCastingBaseAddress)) { return; }

            string baseUrl = App.Default.twitCastingBaseAddress;

            if (baseUrl.Substring(baseUrl.Length - 1, 1) != "/")
            {
                baseUrl += "/";
            }

            if (TwiCasUser == null) { return; } 

            if (string.IsNullOrEmpty(TwiCasUser.LastMovieId)) { return; }

            // Movieが生きてるか死んでるか。
#if DEBUG == false
            if (TwiCasUser.IsAlive == false)
            {
                return;
            }
#endif
            // 最新配信の取得
            string url = baseUrl + $"movies/{TwiCasUser.LastMovieId}/comments?&limit=10";
            var client = new ServiceHttpClient(url, ServiceHttpClient.RequestType.commentInfo);
            var ret = client.Get();
            TwitCastingCommentRoot commentsJson = null;

            string LastTwitCastingName = "";
            if (ret != null)
            {
                //Jsonのデシリアライズ。LastMovieIdの取得
                commentsJson = JsonConvert.DeserializeObject<TwitCastingCommentRoot>(ret.ToString());
                if (commentsJson.Comments.Count < 1)
                {
                    return;
                }
                NowTwitCastingComment = commentsJson.Comments[0].Message;

                //行頭が＠の場合。返信の宛先と判断し除去
                Match match = Regex.Match(NowTwitCastingComment, @"[^@]");
                if (match.Success)
                {
                    //
                    match = Regex.Match(NowTwitCastingComment, @"[' ']");
                    if (match.Success)
                    {
                        NowTwitCastingComment = NowTwitCastingComment.Substring(match.Index);
                    }
                }
            }
            else
            {
                return;
            }

            if (commentsJson.AllCount == commentCounter)
            {
                return;
            }

            if (App.Default.readName)
            {
                LastTwitCastingName = commentsJson.Comments[0].FromUser.Name;
                NowTwitCastingComment = LastTwitCastingName + " " + NowTwitCastingComment;
            }
            else
            {
                LastTwitCastingName = "ツイキャスユーザ";
            }

            if (string.IsNullOrEmpty(NowTwitCastingComment))
            {
                return;
            }

            NowTwitCastingComment = LastTwitCastingName + ":" + NowTwitCastingComment;

            DateTime lastDt = DateTime.Now;
            DateTime newDt = DateTime.Now;
            CommentObject CommentObj = new CommentObject(NowTwitCastingComment, LastTwitCastingComment, App.Default.useTwitcasting)
            {
                LastCommentTime = lastDt,
                NowCommentTime = newDt
            };

            //空行でない & 連続同一コメントでないこと。
            if (CommentObj.CanSpeech)
            {
                //キューにぶっ込む。
                CommentQue.TryAdd(CommentObj);
            }

            commentCounter = commentsJson.AllCount;
            LastTwitCastingComment = NowTwitCastingComment;
        }

        private async Task TaskTryTakeAndSpeech()
        {
            await Task.Run(async () => {
                while (true)
                {
                    await Task.Delay(50);

                    if (CommentQue.Count < 1) { continue; }
                    CommentQue.TryTake(out CommentObject commentObj, 50);
                    if (commentObj == null)
                    {
                        continue;
                    }

                    //多分引っかからないとは思いつつ。
                    if (string.IsNullOrEmpty(commentObj.Comment))
                    {
                        continue;
                    }

                    //メッセージ（情報）を鳴らす
                    TimeSpan commentDiff = commentObj.NowCommentTime - commentObj.LastCommentTime;

                    if (commentObj.ChimeFlg)
                    {
                        if (commentDiff.TotalSeconds > 5)
                        {
                            var chimePath = @"C:\windows\media\windows background.wav";
                            if (File.Exists(chimePath))
                            {

                                var waveReader = new WaveFileReader(chimePath);
                                var waveOut = new WaveOut
                                {
                                    DeviceNumber = App.Default.OutputDevice,
                                };
                                waveOut.Init(waveReader);
                                waveOut.Play();
                            }
                            else
                            {
                                //チャイムは非同期で鳴らす。
                                SystemSounds.Asterisk.Play();
                            }
                            await Task.Delay(100);
                        }
                    }

                    if (commentObj.SpeechFlg == false)
                    {
                        continue;
                    }

                    if (App.Default.UseVoiceVox == true && commentObj.Lang == 0)
                    {
                        SpeechByVOICEVOX(commentObj);
                        continue;
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
                    synth.Speak(commentObj.Comment);
                }
            });
        }

        private static async void SpeechByVOICEVOX(CommentObject commentObj)
        {
            string baseUrl = App.Default.VoiceVoxAddress;
            string url;

            if (string.IsNullOrEmpty(baseUrl))
            {
                //たまに消えるよね、君。
                baseUrl = "http://127.0.0.1:50021";
            }

            if (baseUrl.Substring(baseUrl.Length - 1, 1) != "/")
            {
                baseUrl += "/";
            }

            //クエリー作成
            url = baseUrl + $"audio_query?text='{commentObj.Comment}'&speaker={commentObj.StyleId}";

            var client = new ServiceHttpClient(url, ServiceHttpClient.RequestType.none);
            string QueryResponce = "";

            var ret = client.Post(ref QueryResponce, "");

            if (ret == null)
            {
                return;
            }

            //音声合成
            var queryJson = JsonConvert.DeserializeObject<AccentPhasesRoot>(QueryResponce.ToString());
            queryJson.VolumeScale = App.Default.Volume;
            queryJson.SpeedScale = commentObj.SpeedScale;
            QueryResponce = JsonConvert.SerializeObject(queryJson);

            if (ret.StatusCode.Equals(HttpStatusCode.OK))
            {
                url = baseUrl + $"synthesis?speaker={commentObj.StyleId}";
                client = new ServiceHttpClient(url, ServiceHttpClient.RequestType.none);

                string wavFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                ret = client.Post(ref QueryResponce, wavFile);
                if (ret.StatusCode.Equals(HttpStatusCode.OK))
                {
                    //再生する。
                    var waveReader = new WaveFileReader(wavFile);
                    var waveOut = new WaveOut
                    {
                        DeviceNumber = App.Default.OutputDevice,
                    };
                    waveOut.Init(waveReader);
                    waveOut.Play();

                    if (App.Default.PlayAsync)
                    {
                        //完全に待たないとなると、次のループ処理でバツっと切られちゃうのでねぇ。
                        //非同期なのに2秒ほど待機にしてみた。
                        Thread.Sleep(2000);
                        //非同期
                        //player.Play();
                    }
                    else
                    {
                        //SoundPlayer player = new SoundPlayer(wavFile);
                        //同期
                        //player.PlaySync();
                        while (waveOut.PlaybackState == PlaybackState.Playing)
                        {
                            await Task.Delay(50);
                        }
                    }
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
            AppConfig apConf = new AppConfig{ };
            if (apConf.ShowDialog() == DialogResult.OK)
            {
                App.Default.linkWaveFilePath = apConf.linkWaveFilePath;
                App.Default.waitTiming = apConf.waitTiming;
                App.Default.cutLength = apConf.cutLength;
                App.Default.VoiceVoxAddress = apConf.voiceVoxAddress;
                App.Default.VoiceVoxPath = apConf.voiceVoxPath;
                App.Default.Volume = apConf.volume;
                App.Default.PlayAsync = apConf.playAsync;
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
            Process.Start("https://github.com/dhamaoka/SyncRoomChatTool/wiki");
        }

        private void MenuSettingTwitcasting_Click(object sender, EventArgs e)
        {
            Twitcasting twitCasting = new Twitcasting{ };
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

        private void MenuWindowTopMost_Click(object sender, EventArgs e)
        {
            App.Default.WindowTopMost = MenuWindowTopMost.Checked;
            ActiveForm.TopMost = MenuWindowTopMost.Checked;
            App.Default.Save();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length < 1)
            {
                return;
            }
            // 候補リストに項目を追加
            string newItem = textBox1.Text.Trim();
            if (!String.IsNullOrEmpty(newItem) && !autoCompList.Contains(newItem))
            {
                autoCompList.Add(newItem);
            }

            //ここにSyncRoomのチャットにテキストを流す処理を入れる。
            TargetProcess targetProc = new TargetProcess("SYNCROOM");
            if (targetProc.IsAlive)
            {
                IntPtr chatwHnd = ui.FindHWndByCaptionAndProcessID("チャット", targetProc.Id);

                if (chatwHnd == IntPtr.Zero)
                {
                    // チャットウィンドウの起動
                    IntPtr SyncwHnd = ui.FindHWndByCaptionAndProcessID("SYNCROOM", targetProc.Id);
                    
                    if (SyncwHnd == IntPtr.Zero)
                    {
                        return;
                    }                  

                    AutomationElement mainWindow = AutomationElement.FromHandle(SyncwHnd);

                    AutomationElement buttonElement = ui.GetButtonElement(mainWindow, "チャット");
                    if (buttonElement.Current.Name != "チャット")
                    {
                        return;
                    }

                    InvokePattern bt = ui.GetInvokePattern(buttonElement);
                    bt.Invoke();
                }

                //チャットウィンドウ再取得
                chatwHnd = ui.FindHWndByCaptionAndProcessID("チャット", targetProc.Id);
                if (chatwHnd != IntPtr.Zero)
                {
                    AutomationElement chatWindow = AutomationElement.FromHandle(chatwHnd);

                    AutomationElement chatInput = ui.GetEditElement(chatWindow, "チャット入力");
                    if (chatInput.Current.Name != "チャット入力")
                    {
                        return;
                    }

                    ValuePattern vpInput = ui.GetValuePattern(chatInput);
                    if (vpInput == null) { return; }
                    vpInput.SetValue(textBox1.Text);
                    ui.SendReturn(chatwHnd);

                    this.Activate();
                    textBox1.Text = string.Empty;
                }
            }
        }

        private void TextBox1_Enter(object sender, EventArgs e)
        {
            TargetProcess targetProc = new TargetProcess("SYNCROOM");
            if (targetProc.IsAlive)
            {
                IntPtr chatwHnd = ui.FindHWndByCaptionAndProcessID("チャット", targetProc.Id);
                if (chatwHnd != IntPtr.Zero)
                {
                    AutomationElement chatWindow = AutomationElement.FromHandle(chatwHnd);
                    AutomationElement chatInput = ui.GetEditElement(chatWindow, "チャット入力");
                    ValuePattern vpInput = ui.GetValuePattern(chatInput);
                    if (vpInput == null)
                    {
                        return;
                    }
                    ui.SendMinimized(chatwHnd);
                }
            }
        }

        private void MenuOpenLink_Click(object sender, EventArgs e)
        {
            App.Default.OpenLink = MenuOpenLink.Checked;
            App.Default.Save();
        }

        private void ToolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            App.Default.OutputDevice = toolStripComboBox1.SelectedIndex;
            App.Default.Save();
        }
    }
}
