using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KMHooker;
using KMHooker.WinApi;
using System.Runtime.InteropServices;

namespace DoubleClickCopy
{
    public partial class frmMain : Form
    {
        private readonly MouseHookListener m_MouseHookManager;
        private readonly KeyboardHookListener m_KeyboardHookManager;

        public frmMain()
        {
            InitializeComponent();
            this.Icon = Properties.Resources.ApplicationIcon128;
            m_MouseHookManager = new MouseHookListener(new GlobalHooker());
            m_MouseHookManager.Enabled = true;
            m_MouseHookManager.MouseDoubleClick += MouseDoubleClicked;

            m_KeyboardHookManager = new KeyboardHookListener(new GlobalHooker());
            m_KeyboardHookManager.Enabled = true;
            // m_KeyboardHookManager.KeyPress += HookManager_KeyPress;
            m_KeyboardHookManager.KeyDown += HandleKeyDown;
            
            Hooker hook = new GlobalHooker();
            m_KeyboardHookManager.Replace(hook);
        }

        bool controlKeyPressed = false;
        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode==Keys.LControlKey)
            {
                controlKeyPressed = true;
            }

            if (e.KeyCode == Keys.Add && e.Modifiers == Keys.Control)
            {
                e.Handled = true;
                GetTextFromControlAtMousePosition();
            }

            if (e.KeyCode == Keys.Subtract && e.Modifiers == Keys.Control)
            {
                e.Handled = true;
                SetTextFromControlAtMousePosition();
            }
        }


        private void MouseDoubleClicked(object sender, MouseEventArgs e)
        {
            try
            {
                if (controlKeyPressed)
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        GetTextFromControlAtMousePosition();
                        controlKeyPressed = false;
                    }

                    if (e.Button == MouseButtons.Right)
                    {
                        SetTextFromControlAtMousePosition();
                        controlKeyPressed = false;
                    }
                }
               

            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
        }

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out Point pt);

        [DllImport("user32.dll", EntryPoint = "WindowFromPoint", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr WindowFromPoint(Point pt);

        //Get the text of the control at the mouse position
        private string GetTextFromControlAtMousePosition()
        {
            try
            {
                Point p;
                if (GetCursorPos(out p))
                {
                    IntPtr ptr = WindowFromPoint(p);
                    if (ptr != IntPtr.Zero)
                    {
                        SendCtrlC(ptr);
                        //return GetText(ptr);
                    }
                }
                return "";
            }
            catch (Exception exp)
            {
                return exp.Message;
            }
        }


        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private void SendCtrlC(IntPtr hWnd)
        {
            uint KEYEVENTF_KEYUP = 2;
            byte VK_CONTROL = 0x11;
            SetForegroundWindow(hWnd);
            keybd_event(VK_CONTROL, 0, 0, 0);
            keybd_event(0x43, 0, 0, 0); //Send the C key (43 is "C")
            keybd_event(0x43, 0, KEYEVENTF_KEYUP, 0);
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, 0);// 'Left Control Up
        }

        //Set the text of the control at the mouse position
        private string SetTextFromControlAtMousePosition()
        {
            try
            {
                Point p;
                if (GetCursorPos(out p))
                {
                    IntPtr ptr = WindowFromPoint(p);
                    if (ptr != IntPtr.Zero)
                    {
                        SendCtrlV(ptr);
                    }
                }
                return "";
            }
            catch (Exception exp)
            {
                return exp.Message;
            }
        }

        [DllImport("user32.dll")]
        static extern short MapVirtualKey(int wCode, int wMapType);
        private void SendCtrlV(IntPtr hWnd)
        {
            keybd_event((int)Keys.ControlKey, (byte)MapVirtualKey((int)Keys.ControlKey, 0), 0, 0); // Control Down

            keybd_event((int)Keys.V, (byte)MapVirtualKey((int)Keys.V, 0), 0, 0); // V Down
            keybd_event((int)Keys.V, (byte)MapVirtualKey((int)Keys.V, 0), 2, 0); // V Up

            keybd_event((int)Keys.ControlKey, (byte)MapVirtualKey((int)Keys.ControlKey, 0), 2, 0); // Control Up
        }

        private void chkToggleHandleDoubleClick_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            if (chk.CheckState==CheckState.Checked)
            {
                m_MouseHookManager.MouseDoubleClick += MouseDoubleClicked;
            }
            else
            {
                m_MouseHookManager.MouseDoubleClick -= MouseDoubleClicked;
            }
        }
    }
}
