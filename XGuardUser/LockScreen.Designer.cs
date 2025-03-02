namespace XGuardUser
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
            TimerLabel = new Label();
            SuspendLayout();
            // 
            // TimerLabel
            // 
            TimerLabel.Dock = DockStyle.Fill;
            TimerLabel.Font = new Font("Consolas", 72F, FontStyle.Regular, GraphicsUnit.Point, 204);
            TimerLabel.ForeColor = Color.FromArgb(113, 128, 147);
            TimerLabel.Location = new Point(0, 0);
            TimerLabel.Name = "TimerLabel";
            TimerLabel.Size = new Size(800, 450);
            TimerLabel.TabIndex = 0;
            TimerLabel.Text = "30";
            TimerLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // LockScreen
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(47, 54, 64);
            ClientSize = new Size(800, 450);
            Controls.Add(TimerLabel);
            FormBorderStyle = FormBorderStyle.None;
            Name = "LockScreen";
            Text = "LockScreen";
            TopMost = true;
            WindowState = FormWindowState.Maximized;
            ResumeLayout(false);
        }

        #endregion

        public Label TimerLabel;
    }
}
