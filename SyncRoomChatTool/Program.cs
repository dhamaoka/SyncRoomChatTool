using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using System.Windows.Automation;

namespace SyncRoomChatTool
{

    public class UIAutomationLib
    {
        //readonly string ModuleName = "UIAutomationLib";

        [DllImport("user32.dll", EntryPoint = "FindWindowEx", CharSet = CharSet.Auto)]
        static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern bool SendMessage(IntPtr hWnd, uint Msg, int wParam, StringBuilder lParam);

        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SendMessage(int hWnd, int Msg, int wparam, int lparam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint msg, int wParam, string lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        const int WM_GETTEXT = 0x000D;
        const int WM_SETTEXT = 0x000C;
        const int WM_GETTEXTLENGTH = 0x000E;

        public string GetControlText(IntPtr hWnd)
        {
            StringBuilder controlText = new StringBuilder();
            Int32 size = SendMessage((int)hWnd, WM_GETTEXTLENGTH, 0, 0).ToInt32();
            if (size > 0)
            {
                controlText = new StringBuilder(size + 1);
                SendMessage(hWnd, (int)WM_GETTEXT, controlText.Capacity, controlText);
            }
            return controlText.ToString();
        }
        public IntPtr FindHWndByCaptionAndProcessID(string windowTitle, int ProcessID)
        {
            IntPtr retIntPtr = IntPtr.Zero;
            int maxCount = 9999;
            int ct = 0;
            IntPtr prevChild = IntPtr.Zero;
            IntPtr currChild = IntPtr.Zero;
            while (true && ct < maxCount)
            {
                currChild = FindWindowEx(IntPtr.Zero, prevChild, null, null);
                if (currChild == IntPtr.Zero) break;
                if (IsWindowVisible(currChild))
                {
                    if (GetControlText(currChild).Contains(windowTitle))
                    {
                        uint procID = 0;
                        GetWindowThreadProcessId(currChild, out procID);
                        if (procID == ProcessID)
                        {
                            retIntPtr = currChild;
                            break;
                        }
                    }
                }
                prevChild = currChild;
                ++ct;
            }
            return retIntPtr;
        }

        //指定されたプロセスのMainFramに関するAutomationElementを取得
        public AutomationElement GetMainFrameElement(Process p)
        {
            return AutomationElement.FromHandle(p.MainWindowHandle);
        }

        // チャット画面に強制フォーカス
        public void ForceFocus(AutomationElement rootElement)
        {
            AutomationElementCollection allElements = rootElement.FindAll(TreeScope.Element | TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, ""));

            foreach (AutomationElement el in allElements)
            {
                if (el.Current.Name.Contains("チャット画面"))
                {
                    el.SetFocus();
                    break;
                }
            }
        }

        // チャットログの中身を取得
        public AutomationElement GetElement(AutomationElement rootElement)
        {
            AutomationElementCollection allElements = rootElement.FindAll(TreeScope.Element | TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, ""));

            //見つからなかったら、渡されたルートエレメントを返す。
            AutomationElement returnElement = rootElement;

            foreach (AutomationElement el in allElements)
            {

                //Debug.WriteLine(el.Current.Name);
                if (el.Current.Name.Contains("チャットログ"))
                {
                    returnElement = el;
                    break;
                }
            }
            return returnElement;
        }
    }

    public class TargetProcess
    {
        private readonly Process targetProcess;

        public TargetProcess(string pName) {
            Process[] ps = System.Diagnostics.Process.GetProcessesByName(pName);
            foreach (Process p in ps)
            {
                targetProcess = p;
                break;
            }
        }

        public bool IsAlive()
        {
            if (targetProcess == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public int Id()
        {
            return targetProcess.Id;
        }


        public Process Proc()
        {
            return targetProcess;
        }

        public IntPtr Handle()
        {
            return targetProcess.Handle; 
        }
    }

    internal static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
