using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Automation;

namespace SyncRoomChatTool
{
    public class UIAutomationLib
    {
        //readonly string ModuleName = "UIAutomationLib";

        [DllImport("user32.dll", EntryPoint = "FindWindowEx", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

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
            while (true && ct < maxCount)
            {
                IntPtr currChild = FindWindowEx(IntPtr.Zero, prevChild, null, null);
                if (currChild == IntPtr.Zero) break;
                if (IsWindowVisible(currChild))
                {
                    if (GetControlText(currChild).Contains(windowTitle))
                    {
                        GetWindowThreadProcessId(currChild, out uint procID);
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

        // チャットログの中身を取得
        public AutomationElement GetElement(AutomationElement rootElement)
        {
            AutomationElementCollection allElements = rootElement.FindAll(TreeScope.Element | TreeScope.Descendants, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit));

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

        public ValuePattern GetValuePattern(
            AutomationElement targetControl)
        {
            ValuePattern valuePattern;
            try
            {
                valuePattern =
                    targetControl.GetCurrentPattern(
                    ValuePattern.Pattern)
                    as ValuePattern;
            }
            // Object doesn't support the ValuePattern control pattern
            catch (InvalidOperationException)
            {
                return null;
            }

            return valuePattern;
        }
    }
}
