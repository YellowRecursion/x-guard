namespace XGuardLockScreen
{
    partial class LockScreen
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // LockScreen
            // 
            AutoScaleDimensions = new SizeF(12F, 30F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(47, 54, 64);
            ClientSize = new Size(1200, 675);
            FormBorderStyle = FormBorderStyle.None;
            Margin = new Padding(4);
            Name = "LockScreen";
            Text = "LockScreen";
            TopMost = true;
            WindowState = FormWindowState.Maximized;
            FormClosed += LockScreen_FormClosed;
            Load += LockScreen_Load;
            ResumeLayout(false);
        }

        #endregion
    }
}
