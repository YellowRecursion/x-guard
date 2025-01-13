namespace XGuard
{
    public partial class OverlayForm : Form
    {
        public OverlayForm()
        {
            InitializeComponent();
        }

        public void SetCloseButtonTimerValue(int value)
        {
            if (value <= 0)
            {
                value = 0;
            }

            CloseButton.Enabled = value == 0;

            if (value > 0)
            {
                CloseButton.Text = $"Close ({value})";
            }
            else
            {
                CloseButton.Text = $"Close";
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            if (!CloseButton.Enabled) return;

            Program.ShowOverlay = false;
        }
    }
}
