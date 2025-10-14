namespace MouseClickLocker
{
    partial class SettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            label1 = new Label();
            trackBarMarkerXOffset = new TrackBar();
            label2 = new Label();
            trackBarMarkerYOffset = new TrackBar();
            label3 = new Label();
            labelClickDelayMS = new Label();
            buttonOK = new Button();
            buttonCancel = new Button();
            trackBarClickLockDelayMS = new TrackBar();
            ((System.ComponentModel.ISupportInitialize)trackBarMarkerXOffset).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBarMarkerYOffset).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBarClickLockDelayMS).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(29, 25);
            label1.Name = "label1";
            label1.Size = new Size(60, 15);
            label1.TabIndex = 0;
            label1.Text = "横位置(&H)";
            // 
            // trackBarMarkerXOffset
            // 
            trackBarMarkerXOffset.Location = new Point(106, 25);
            trackBarMarkerXOffset.Maximum = 64;
            trackBarMarkerXOffset.Minimum = -64;
            trackBarMarkerXOffset.Name = "trackBarMarkerXOffset";
            trackBarMarkerXOffset.Size = new Size(231, 45);
            trackBarMarkerXOffset.TabIndex = 1;
            trackBarMarkerXOffset.TickStyle = TickStyle.None;
            trackBarMarkerXOffset.ValueChanged += TrackBarMarkerXOffset_ValueChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(29, 70);
            label2.Name = "label2";
            label2.Size = new Size(58, 15);
            label2.TabIndex = 2;
            label2.Text = "縦位置(&V)";
            // 
            // trackBarMarkerYOffset
            // 
            trackBarMarkerYOffset.Location = new Point(106, 70);
            trackBarMarkerYOffset.Maximum = 64;
            trackBarMarkerYOffset.Minimum = -64;
            trackBarMarkerYOffset.Name = "trackBarMarkerYOffset";
            trackBarMarkerYOffset.Size = new Size(231, 45);
            trackBarMarkerYOffset.TabIndex = 3;
            trackBarMarkerYOffset.TickStyle = TickStyle.None;
            trackBarMarkerYOffset.ValueChanged += TrackBarMarkerYOffset_ValueChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(29, 115);
            label3.Name = "label3";
            label3.Size = new Size(77, 15);
            label3.TabIndex = 4;
            label3.Text = "長押し時間(&T)";
            // 
            // labelClickDelayMS
            // 
            labelClickDelayMS.AutoSize = true;
            labelClickDelayMS.Location = new Point(343, 115);
            labelClickDelayMS.Name = "labelClickDelayMS";
            labelClickDelayMS.Size = new Size(34, 15);
            labelClickDelayMS.TabIndex = 6;
            labelClickDelayMS.Text = "ミリ秒";
            // 
            // buttonOK
            // 
            buttonOK.DialogResult = DialogResult.OK;
            buttonOK.Location = new Point(262, 166);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new Size(75, 30);
            buttonOK.TabIndex = 7;
            buttonOK.Text = "OK";
            buttonOK.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Location = new Point(346, 166);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(75, 30);
            buttonCancel.TabIndex = 8;
            buttonCancel.Text = "キャンセル";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // trackBarClickLockDelayMS
            // 
            trackBarClickLockDelayMS.Location = new Point(106, 115);
            trackBarClickLockDelayMS.Maximum = 3000;
            trackBarClickLockDelayMS.Minimum = 500;
            trackBarClickLockDelayMS.Name = "trackBarClickLockDelayMS";
            trackBarClickLockDelayMS.Size = new Size(231, 45);
            trackBarClickLockDelayMS.TabIndex = 9;
            trackBarClickLockDelayMS.TickStyle = TickStyle.None;
            trackBarClickLockDelayMS.Value = 500;
            trackBarClickLockDelayMS.ValueChanged += TrackBarClickLockDelayMS_ValueChanged;
            // 
            // SettingsForm
            // 
            AcceptButton = buttonOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new Size(441, 213);
            Controls.Add(trackBarClickLockDelayMS);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
            Controls.Add(labelClickDelayMS);
            Controls.Add(label3);
            Controls.Add(trackBarMarkerYOffset);
            Controls.Add(label2);
            Controls.Add(trackBarMarkerXOffset);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SettingsForm";
            Text = "マウスクリックロックの設定";
            ((System.ComponentModel.ISupportInitialize)trackBarMarkerXOffset).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBarMarkerYOffset).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBarClickLockDelayMS).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TrackBar trackBarMarkerXOffset;
        private Label label2;
        private TrackBar trackBarMarkerYOffset;
        private Label label3;
        private Button buttonOK;
        private Button buttonCancel;
        private TrackBar trackBarClickLockDelayMS;
        private Label labelClickDelayMS;
    }
}