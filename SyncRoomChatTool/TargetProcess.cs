using System;
using System.Diagnostics;

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
    }
}
