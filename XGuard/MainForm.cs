using System.Runtime.InteropServices;
using XGuard.Properties;

namespace XGuard
{
    public partial class MainForm : Form
    {
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr one, int two, int three, int four);

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void HeadPanel_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(Handle, 0x112, 0xf012, 0);
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            if (TerminationModule.IsTerminatable)
            {
                Application.Exit();
            }
            else
            {
                MinimizeButton_Click(sender, e);
            }
        }

        private void MaximizeButton_Click(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                WindowState = FormWindowState.Maximized;
                MaximizeButton.Image = Resources.NormalScreen;
            }
            else
            {
                WindowState = FormWindowState.Normal;
                MaximizeButton.Image = Resources.FullScreen;
            }
        }

        private void MinimizeButton_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;

            NotifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            NotifyIcon.BalloonTipTitle = "XGuard";
            NotifyIcon.BalloonTipText = "XGuard working in the background";
            NotifyIcon.ShowBalloonTip(3);
        }

        private void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            WindowState = FormWindowState.Normal;
            Activate();
        }

        private void LogsTextBox_TextChanged(object sender, EventArgs e)
        {
            LogsTextBox.SelectionStart = LogsTextBox.Text.Length;
            LogsTextBox.ScrollToCaret();
        }
    }
}
