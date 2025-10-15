namespace MouseClickLocker
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
            notifyIcon = new NotifyIcon(components);
            contextMenuStrip = new ContextMenuStrip(components);
            aboutToolStripMenuItem = new ToolStripMenuItem();
            settingsToolStripMenuItem = new ToolStripMenuItem();
            isClickLockEnabledToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            quitToolStripMenuItem = new ToolStripMenuItem();
            contextMenuStrip.SuspendLayout();
            SuspendLayout();
            // 
            // notifyIcon
            // 
            notifyIcon.ContextMenuStrip = contextMenuStrip;
            notifyIcon.Icon = (Icon)resources.GetObject("notifyIcon.Icon");
            notifyIcon.Text = "Mouse Click Locker";
            notifyIcon.Visible = true;
            // 
            // contextMenuStrip
            // 
            contextMenuStrip.Items.AddRange(new ToolStripItem[] { aboutToolStripMenuItem, settingsToolStripMenuItem, isClickLockEnabledToolStripMenuItem, toolStripSeparator1, quitToolStripMenuItem });
            contextMenuStrip.Name = "contextMenuStrip";
            contextMenuStrip.Size = new Size(196, 98);
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new Size(195, 22);
            aboutToolStripMenuItem.Text = "このアプリについて(&A)...";
            aboutToolStripMenuItem.Click += AboutToolStripMenuItem_Click;
            // 
            // settingsToolStripMenuItem
            // 
            settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            settingsToolStripMenuItem.Size = new Size(195, 22);
            settingsToolStripMenuItem.Text = "設定(&S)...";
            settingsToolStripMenuItem.Click += SettingsToolStripMenuItem_Click;
            // 
            // isClickLockEnabledToolStripMenuItem
            // 
            isClickLockEnabledToolStripMenuItem.Checked = true;
            isClickLockEnabledToolStripMenuItem.CheckState = CheckState.Checked;
            isClickLockEnabledToolStripMenuItem.Name = "isClickLockEnabledToolStripMenuItem";
            isClickLockEnabledToolStripMenuItem.Size = new Size(195, 22);
            isClickLockEnabledToolStripMenuItem.Text = "クリックロックを有効にする";
            isClickLockEnabledToolStripMenuItem.Click += IsClickLockEnabledToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(192, 6);
            // 
            // quitToolStripMenuItem
            // 
            quitToolStripMenuItem.Name = "quitToolStripMenuItem";
            quitToolStripMenuItem.Size = new Size(195, 22);
            quitToolStripMenuItem.Text = "終了(&Q)";
            quitToolStripMenuItem.Click += QuitToolStripMenuItem_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(184, 61);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "MainForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Text = "MouseClickLoker";
            WindowState = FormWindowState.Minimized;
            Load += MainForm_Load;
            contextMenuStrip.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private NotifyIcon notifyIcon;
        private ContextMenuStrip contextMenuStrip;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private ToolStripMenuItem settingsToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem quitToolStripMenuItem;
        private ToolStripMenuItem isClickLockEnabledToolStripMenuItem;
    }
}
