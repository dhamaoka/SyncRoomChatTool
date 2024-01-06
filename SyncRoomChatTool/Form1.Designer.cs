namespace SyncRoomChatTool
{
    partial class Form1
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param Name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.MenuFile = new System.Windows.Forms.ToolStripMenuItem();
            this.menuClose = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuTool = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuFont = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuOption = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuWindowTopMost = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuOpenLink = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuSpeech = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuEnebleSpeech = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuUseVoiceVox = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuTwitcasting = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuEnableTwitcasting = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuSettingTwitcasting = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_Help = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripComboBox1 = new System.Windows.Forms.ToolStripComboBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel4 = new System.Windows.Forms.ToolStripStatusLabel();
            this.fontDialog1 = new System.Windows.Forms.FontDialog();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuFile,
            this.MenuTool,
            this.MenuSpeech,
            this.MenuTwitcasting,
            this.Menu_Help,
            this.toolStripComboBox1});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(940, 32);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // MenuFile
            // 
            this.MenuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuClose});
            this.MenuFile.Font = new System.Drawing.Font("Yu Gothic UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.MenuFile.Name = "MenuFile";
            this.MenuFile.Size = new System.Drawing.Size(90, 28);
            this.MenuFile.Text = "ファイル(&F)";
            // 
            // menuClose
            // 
            this.menuClose.Name = "menuClose";
            this.menuClose.Size = new System.Drawing.Size(155, 28);
            this.menuClose.Text = "閉じる(&E)";
            this.menuClose.Click += new System.EventHandler(this.MenuClose_Click);
            // 
            // MenuTool
            // 
            this.MenuTool.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuFont,
            this.MenuOption,
            this.toolStripSeparator1,
            this.MenuWindowTopMost,
            this.MenuOpenLink});
            this.MenuTool.Font = new System.Drawing.Font("Yu Gothic UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.MenuTool.Name = "MenuTool";
            this.MenuTool.Size = new System.Drawing.Size(81, 28);
            this.MenuTool.Text = "ツール(&T)";
            // 
            // MenuFont
            // 
            this.MenuFont.Name = "MenuFont";
            this.MenuFont.Size = new System.Drawing.Size(234, 28);
            this.MenuFont.Text = "フォント(&F)";
            this.MenuFont.Click += new System.EventHandler(this.MenuFont_Click);
            // 
            // MenuOption
            // 
            this.MenuOption.Name = "MenuOption";
            this.MenuOption.Size = new System.Drawing.Size(234, 28);
            this.MenuOption.Text = "設定(&O)";
            this.MenuOption.Click += new System.EventHandler(this.MenuOption_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(231, 6);
            // 
            // MenuWindowTopMost
            // 
            this.MenuWindowTopMost.CheckOnClick = true;
            this.MenuWindowTopMost.Name = "MenuWindowTopMost";
            this.MenuWindowTopMost.Size = new System.Drawing.Size(234, 28);
            this.MenuWindowTopMost.Text = "最前面固定(&T)";
            this.MenuWindowTopMost.Click += new System.EventHandler(this.MenuWindowTopMost_Click);
            // 
            // MenuOpenLink
            // 
            this.MenuOpenLink.CheckOnClick = true;
            this.MenuOpenLink.Name = "MenuOpenLink";
            this.MenuOpenLink.Size = new System.Drawing.Size(234, 28);
            this.MenuOpenLink.Text = "リンク自動オープン(&L)";
            this.MenuOpenLink.Click += new System.EventHandler(this.MenuOpenLink_Click);
            // 
            // MenuSpeech
            // 
            this.MenuSpeech.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuEnebleSpeech,
            this.MenuUseVoiceVox});
            this.MenuSpeech.Font = new System.Drawing.Font("Yu Gothic UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.MenuSpeech.Name = "MenuSpeech";
            this.MenuSpeech.Size = new System.Drawing.Size(92, 28);
            this.MenuSpeech.Text = "スピーチ(&S)";
            // 
            // MenuEnebleSpeech
            // 
            this.MenuEnebleSpeech.Checked = true;
            this.MenuEnebleSpeech.CheckOnClick = true;
            this.MenuEnebleSpeech.CheckState = System.Windows.Forms.CheckState.Checked;
            this.MenuEnebleSpeech.Name = "MenuEnebleSpeech";
            this.MenuEnebleSpeech.Size = new System.Drawing.Size(239, 28);
            this.MenuEnebleSpeech.Text = "スピーチさせる(&E)";
            this.MenuEnebleSpeech.Click += new System.EventHandler(this.Menu_EnebleSpeech_Click);
            // 
            // MenuUseVoiceVox
            // 
            this.MenuUseVoiceVox.Checked = true;
            this.MenuUseVoiceVox.CheckOnClick = true;
            this.MenuUseVoiceVox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.MenuUseVoiceVox.Name = "MenuUseVoiceVox";
            this.MenuUseVoiceVox.Size = new System.Drawing.Size(239, 28);
            this.MenuUseVoiceVox.Text = "VOICEVOXを使う(&V)";
            this.MenuUseVoiceVox.Click += new System.EventHandler(this.Menu_VoiceVox_Click);
            // 
            // MenuTwitcasting
            // 
            this.MenuTwitcasting.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuEnableTwitcasting,
            this.MenuSettingTwitcasting});
            this.MenuTwitcasting.Font = new System.Drawing.Font("Yu Gothic UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.MenuTwitcasting.Name = "MenuTwitcasting";
            this.MenuTwitcasting.Size = new System.Drawing.Size(141, 28);
            this.MenuTwitcasting.Text = "ツイキャス連携(&C)";
            // 
            // MenuEnableTwitcasting
            // 
            this.MenuEnableTwitcasting.CheckOnClick = true;
            this.MenuEnableTwitcasting.Name = "MenuEnableTwitcasting";
            this.MenuEnableTwitcasting.Size = new System.Drawing.Size(213, 28);
            this.MenuEnableTwitcasting.Text = "連携する(&R)";
            this.MenuEnableTwitcasting.Click += new System.EventHandler(this.MenuEnableTwitcasting_Click);
            // 
            // MenuSettingTwitcasting
            // 
            this.MenuSettingTwitcasting.Name = "MenuSettingTwitcasting";
            this.MenuSettingTwitcasting.Size = new System.Drawing.Size(213, 28);
            this.MenuSettingTwitcasting.Text = "ツイキャス設定(&O)";
            this.MenuSettingTwitcasting.Click += new System.EventHandler(this.MenuSettingTwitcasting_Click);
            // 
            // Menu_Help
            // 
            this.Menu_Help.Font = new System.Drawing.Font("Yu Gothic UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.Menu_Help.Name = "Menu_Help";
            this.Menu_Help.Size = new System.Drawing.Size(87, 28);
            this.Menu_Help.Text = "ヘルプ(&H)";
            this.Menu_Help.Click += new System.EventHandler(this.Menu_Help_Click);
            // 
            // toolStripComboBox1
            // 
            this.toolStripComboBox1.Name = "toolStripComboBox1";
            this.toolStripComboBox1.Size = new System.Drawing.Size(250, 28);
            this.toolStripComboBox1.SelectedIndexChanged += new System.EventHandler(this.ToolStripComboBox1_SelectedIndexChanged);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Font = new System.Drawing.Font("Yu Gothic UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel2,
            this.toolStripStatusLabel3,
            this.toolStripStatusLabel4});
            this.statusStrip1.Location = new System.Drawing.Point(0, 560);
            this.statusStrip1.Margin = new System.Windows.Forms.Padding(2);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(940, 22);
            this.statusStrip1.TabIndex = 1;
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 16);
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(0, 16);
            // 
            // toolStripStatusLabel3
            // 
            this.toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            this.toolStripStatusLabel3.Size = new System.Drawing.Size(0, 16);
            // 
            // toolStripStatusLabel4
            // 
            this.toolStripStatusLabel4.Name = "toolStripStatusLabel4";
            this.toolStripStatusLabel4.Size = new System.Drawing.Size(0, 16);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 32);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.richTextBox1);
            this.splitContainer1.Panel1.Padding = new System.Windows.Forms.Padding(4);
            this.splitContainer1.Panel1MinSize = 0;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.textBox1);
            this.splitContainer1.Panel2.Controls.Add(this.button1);
            this.splitContainer1.Panel2.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer1.Panel2.Padding = new System.Windows.Forms.Padding(5);
            this.splitContainer1.Panel2MinSize = 40;
            this.splitContainer1.Size = new System.Drawing.Size(940, 528);
            this.splitContainer1.SplitterDistance = 487;
            this.splitContainer1.SplitterWidth = 1;
            this.splitContainer1.TabIndex = 6;
            // 
            // richTextBox1
            // 
            this.richTextBox1.AutoWordSelection = true;
            this.richTextBox1.BackColor = System.Drawing.SystemColors.Window;
            this.richTextBox1.Cursor = System.Windows.Forms.Cursors.Default;
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox1.Font = new System.Drawing.Font("游ゴシック", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.richTextBox1.Location = new System.Drawing.Point(4, 4);
            this.richTextBox1.Margin = new System.Windows.Forms.Padding(5);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.richTextBox1.ShortcutsEnabled = false;
            this.richTextBox1.Size = new System.Drawing.Size(932, 479);
            this.richTextBox1.TabIndex = 4;
            this.richTextBox1.Text = "";
            this.richTextBox1.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.RichTextBox1_LinkClicked);
            this.richTextBox1.TextChanged += new System.EventHandler(this.RichTextBox1_TextChanged);
            // 
            // textBox1
            // 
            this.textBox1.AutoCompleteCustomSource.AddRange(new string[] {
            "kore",
            "are",
            "dore"});
            this.textBox1.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.textBox1.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox1.Font = new System.Drawing.Font("メイリオ", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.textBox1.ImeMode = System.Windows.Forms.ImeMode.On;
            this.textBox1.Location = new System.Drawing.Point(5, 5);
            this.textBox1.Margin = new System.Windows.Forms.Padding(2);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(930, 37);
            this.textBox1.TabIndex = 0;
            this.textBox1.Enter += new System.EventHandler(this.TextBox1_Enter);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(826, 8);
            this.button1.Margin = new System.Windows.Forms.Padding(2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(80, 30);
            this.button1.TabIndex = 2;
            this.button1.TabStop = false;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Button1_Click);
            // 
            // Form1
            // 
            this.AcceptButton = this.button1;
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(940, 582);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(299, 148);
            this.Name = "Form1";
            this.Text = "SyncRoom読み上げちゃん";
            this.Activated += new System.EventHandler(this.TextBox1_Enter);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolStripMenuItem MenuFile;
        private System.Windows.Forms.ToolStripMenuItem menuClose;
        private System.Windows.Forms.ToolStripMenuItem MenuSpeech;
        public System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        public System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        public System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripMenuItem MenuOption;
        public System.Windows.Forms.MenuStrip menuStrip1;
        public System.Windows.Forms.ToolStripMenuItem MenuTool;
        private System.Windows.Forms.ToolStripMenuItem MenuFont;
        private System.Windows.Forms.FontDialog fontDialog1;
        private System.Windows.Forms.ToolStripMenuItem MenuUseVoiceVox;
        private System.Windows.Forms.ToolStripMenuItem MenuEnebleSpeech;
        private System.Windows.Forms.ToolStripMenuItem Menu_Help;
        private System.Windows.Forms.ToolStripMenuItem MenuTwitcasting;
        private System.Windows.Forms.ToolStripMenuItem MenuEnableTwitcasting;
        private System.Windows.Forms.ToolStripMenuItem MenuSettingTwitcasting;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem MenuWindowTopMost;
        private System.Windows.Forms.SplitContainer splitContainer1;
        public System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel4;
        private System.Windows.Forms.ToolStripMenuItem MenuOpenLink;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBox1;
    }
}

