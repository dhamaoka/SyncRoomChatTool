using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Windows.Automation;
using System.Threading;


namespace SyncRoomChatTool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private bool TargetProcessIsAlive;
        private AutomationElement chatLog;
        private static UIAutomationLib ui = new UIAutomationLib();

        private void Form1_Load(object sender, EventArgs e)
        {
            TargetProcessIsAlive = false;
            _ = CheckProcess("SYNCROOM",TargetProcessIsAlive,chatLog, this.statusStrip1);
        }

        static async Task CheckProcess(string ProcessName, bool TargetProcessIsAlive, AutomationElement chatLog, StatusStrip ststp )
        {

            while (true)
            {
                TargetProcess tp = new TargetProcess(ProcessName);
                await Task.Delay(1000);

                TreeNode tr = new TreeNode();

                TargetProcessIsAlive = tp.IsAlive();

                string toolMessage;
                if (TargetProcessIsAlive)
                {
                    toolMessage = "は起動されています。";
                    try
                    {
                        AutomationElement synroomElement = ui.GetMainFrameElement(tp.Proc());
                        //WalkControlElements(synroomElement, tr);

                        IntPtr chatwHnd = ui.FindHWndByCaptionAndProcessID("チャット", tp.Id());

                        if (chatwHnd != IntPtr.Zero)
                        {
                            AutomationElement chatWindow = AutomationElement.FromHandle(chatwHnd);
                            chatWindow.SetFocus();
                            ststp.Items[1].Text = "チャットログ監視中。";
                            chatLog = ui.GetElement(synroomElement);
                            var arry = chatLog.Current.Name.Split('\u2028');
                        }
                        else
                        {
                            ststp.Items[1].Text = "チャットログ待機中…";
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message.ToString());
                        break;
                    }
                }
                else
                {
                    toolMessage = "は起動されていません。";
                }
                ststp.Items[0].Text = ProcessName + toolMessage;
            }
        }

        private void MenuClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
