using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DAILibWV
{
    public static class Debug
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool LockWindowUpdate(IntPtr hWndLock);

        private static readonly object _sync = new object();

        public static RichTextBox box = null;

        public static void SetBox(RichTextBox rtb)
        {
            box = rtb;
        }

        public static void Log(string s, bool update = true)
        {
            lock (_sync)
            {
                if (box == null)
                    return;
                LockWindowUpdate(box.Parent.Handle);
                box.AppendText(s);
                if (update)
                {
                    LockWindowUpdate(IntPtr.Zero);
                    Application.DoEvents();
                }
            }
        }

        public static void LogLn(string s, bool update = true)
        {
            Log(s + "\n", update);
        }
    }
}
