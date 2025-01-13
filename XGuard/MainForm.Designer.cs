namespace XGuard
{
    partial class MainForm
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            LogsTextBox = new RichTextBox();
            pictureBox1 = new PictureBox();
            HeadPanel = new Panel();
            MinimizeButton = new Button();
            MaximizeButton = new Button();
            CloseButton = new Button();
            NotifyIcon = new NotifyIcon(components);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            HeadPanel.SuspendLayout();
            SuspendLayout();
            // 
            // LogsTextBox
            // 
            LogsTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            LogsTextBox.BackColor = Color.FromArgb(53, 59, 72);
            LogsTextBox.BorderStyle = BorderStyle.None;
            LogsTextBox.Font = new Font("Consolas", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 204);
            LogsTextBox.ForeColor = Color.FromArgb(245, 246, 250);
            LogsTextBox.Location = new Point(0, 40);
            LogsTextBox.Name = "LogsTextBox";
            LogsTextBox.ReadOnly = true;
            LogsTextBox.Size = new Size(800, 560);
            LogsTextBox.TabIndex = 0;
            LogsTextBox.Text = "";
            LogsTextBox.TextChanged += LogsTextBox_TextChanged;
            // 
            // pictureBox1
            // 
            pictureBox1.Dock = DockStyle.Left;
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(0, 0);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(120, 40);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 1;
            pictureBox1.TabStop = false;
            // 
            // HeadPanel
            // 
            HeadPanel.Controls.Add(MinimizeButton);
            HeadPanel.Controls.Add(MaximizeButton);
            HeadPanel.Controls.Add(CloseButton);
            HeadPanel.Controls.Add(pictureBox1);
            HeadPanel.Dock = DockStyle.Top;
            HeadPanel.Location = new Point(0, 0);
            HeadPanel.Name = "HeadPanel";
            HeadPanel.Size = new Size(800, 40);
            HeadPanel.TabIndex = 2;
            HeadPanel.MouseDown += HeadPanel_MouseDown;
            // 
            // MinimizeButton
            // 
            MinimizeButton.Dock = DockStyle.Right;
            MinimizeButton.FlatAppearance.BorderSize = 0;
            MinimizeButton.FlatStyle = FlatStyle.Flat;
            MinimizeButton.ForeColor = Color.FromArgb(245, 246, 250);
            MinimizeButton.Image = Properties.Resources.Minimize;
            MinimizeButton.Location = new Point(620, 0);
            MinimizeButton.Name = "MinimizeButton";
            MinimizeButton.Size = new Size(60, 40);
            MinimizeButton.TabIndex = 4;
            MinimizeButton.UseVisualStyleBackColor = true;
            MinimizeButton.Click += MinimizeButton_Click;
            // 
            // MaximizeButton
            // 
            MaximizeButton.Dock = DockStyle.Right;
            MaximizeButton.FlatAppearance.BorderSize = 0;
            MaximizeButton.FlatStyle = FlatStyle.Flat;
            MaximizeButton.ForeColor = Color.FromArgb(245, 246, 250);
            MaximizeButton.Image = Properties.Resources.FullScreen;
            MaximizeButton.Location = new Point(680, 0);
            MaximizeButton.Name = "MaximizeButton";
            MaximizeButton.Size = new Size(60, 40);
            MaximizeButton.TabIndex = 3;
            MaximizeButton.UseVisualStyleBackColor = true;
            MaximizeButton.Click += MaximizeButton_Click;
            // 
            // CloseButton
            // 
            CloseButton.Dock = DockStyle.Right;
            CloseButton.FlatAppearance.BorderSize = 0;
            CloseButton.FlatStyle = FlatStyle.Flat;
            CloseButton.ForeColor = Color.FromArgb(245, 246, 250);
            CloseButton.Image = Properties.Resources.Close;
            CloseButton.Location = new Point(740, 0);
            CloseButton.Name = "CloseButton";
            CloseButton.Size = new Size(60, 40);
            CloseButton.TabIndex = 2;
            CloseButton.UseVisualStyleBackColor = true;
            CloseButton.Click += CloseButton_Click;
            // 
            // NotifyIcon
            // 
            NotifyIcon.Icon = (Icon)resources.GetObject("NotifyIcon.Icon");
            NotifyIcon.Text = "XGuard";
            NotifyIcon.Visible = true;
            NotifyIcon.MouseClick += NotifyIcon_MouseClick;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(47, 54, 64);
            ClientSize = new Size(800, 600);
            Controls.Add(LogsTextBox);
            Controls.Add(HeadPanel);
            FormBorderStyle = FormBorderStyle.None;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "MainForm";
            ShowInTaskbar = false;
            Text = "XGuard";
            Load += MainForm_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            HeadPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        public RichTextBox LogsTextBox;
        private PictureBox pictureBox1;
        private Panel HeadPanel;
        private Button CloseButton;
        private Button MaximizeButton;
        private Button MinimizeButton;
        private NotifyIcon NotifyIcon;
    }
}
