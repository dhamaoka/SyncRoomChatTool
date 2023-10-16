using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace SyncRoomChatTool
{
    public class TargetProcess
    {
        private readonly Process targetProcess;
        public bool IsAlive = true;
        public int Id = 0;
        public Process Proc;
        public IntPtr Handle;

        public TargetProcess(string pName)
        {
            try
            {
                Process[] ps = Process.GetProcessesByName(pName);
                foreach (Process p in ps)
                {
                    targetProcess = p;
                    break;
                }

                if (targetProcess == null)
                {
                    IsAlive = false;
                    return;
                }

                Id = targetProcess.Id;
                Proc = targetProcess;
                Handle = targetProcess.Handle;
            }
            catch (Exception ex)
            {
                string errMsg = $"エラーが発生しています。{ex.Message}";
                MessageBox.Show(errMsg);
                Application.Exit();
            }
        }
    }
}
