namespace XGuard
{
    partial class OverlayForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OverlayForm));
            Panel = new Panel();
            CloseButton = new Button();
            Label = new Label();
            pictureBox1 = new PictureBox();
            Panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // Panel
            // 
            Panel.Anchor = AnchorStyles.None;
            Panel.BackColor = Color.FromArgb(47, 54, 64);
            Panel.Controls.Add(CloseButton);
            Panel.Controls.Add(Label);
            Panel.Controls.Add(pictureBox1);
            Panel.Location = new Point(200, 150);
            Panel.Name = "Panel";
            Panel.Size = new Size(400, 300);
            Panel.TabIndex = 0;
            // 
            // CloseButton
            // 
            CloseButton.BackColor = Color.FromArgb(64, 115, 158);
            CloseButton.FlatAppearance.BorderSize = 0;
            CloseButton.FlatStyle = FlatStyle.Flat;
            CloseButton.ForeColor = Color.FromArgb(245, 246, 250);
            CloseButton.Location = new Point(130, 235);
            CloseButton.Name = "CloseButton";
            CloseButton.Size = new Size(140, 40);
            CloseButton.TabIndex = 2;
            CloseButton.Text = "Close (59)";
            CloseButton.UseVisualStyleBackColor = false;
            CloseButton.Click += CloseButton_Click;
            // 
            // Label
            // 
            Label.Anchor = AnchorStyles.None;
            Label.AutoSize = true;
            Label.Font = new Font("Verdana", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
            Label.ForeColor = Color.FromArgb(245, 246, 250);
            Label.Location = new Point(92, 180);
            Label.Name = "Label";
            Label.Size = new Size(217, 25);
            Label.TabIndex = 1;
            Label.Text = "XGuard Lock Screen";
            Label.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pictureBox1
            // 
            pictureBox1.Anchor = AnchorStyles.None;
            pictureBox1.Image = Properties.Resources.Secure;
            pictureBox1.Location = new Point(152, 50);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(96, 96);
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // OverlayForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(800, 600);
            Controls.Add(Panel);
            FormBorderStyle = FormBorderStyle.None;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "OverlayForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Overlay";
            TopMost = true;
            Panel.ResumeLayout(false);
            Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel Panel;
        private PictureBox pictureBox1;
        private Label Label;
        private Button CloseButton;
    }
}