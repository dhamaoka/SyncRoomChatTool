using System;
using System.Windows.Forms;

namespace SyncRoomChatTool
{
    public partial class Twitcasting : Form
    {
        public string AccessToken;
        public string TwitcastUserName;
        public bool ReadName;
        public string ClientId;

        public Twitcasting()
        {
            InitializeComponent();
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            AccessToken = txtAccessToken.Text;
            TwitcastUserName = txtTwitcastUserName.Text;
            ReadName = chkReadName.Checked;
            Close();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

            System.Diagnostics.Process.Start($"https://apiv2.twitcasting.tv/oauth2/authorize?client_id={ClientId}&response_type=token");
        }

        private void Twitcasting_Load(object sender, EventArgs e)
        {
            txtAccessToken.Text = App.Default.AccessToken;
            txtTwitcastUserName.Text = App.Default.twitcastUserName;
            chkReadName.Checked = App.Default.readName;
            ClientId = App.Default.clientId;
        }
    }
}
