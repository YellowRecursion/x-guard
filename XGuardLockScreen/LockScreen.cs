using XGuardLockScreen.Utilities;

namespace XGuardLockScreen
{
    public partial class LockScreen : Form
    {
        private KeyboardLocker _locker;

        public bool Prime { get; set; }

        public LockScreen()
        {
            InitializeComponent();
        }

        private void LockScreen_Load(object sender, EventArgs e)
        {
            if (Prime)
                _locker = new KeyboardLocker();
        }

        private void LockScreen_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (Prime)
                _locker.Dispose();
        }
    }
}
