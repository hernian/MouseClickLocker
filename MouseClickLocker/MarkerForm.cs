using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace MouseClickLocker
{
    public partial class MarkerForm : Form
    {
        public MarkerForm()
        {
            InitializeComponent();
            this.AutoScaleMode = AutoScaleMode.None;
            this.FormBorderStyle = FormBorderStyle.None;
            this.ClientSize = new Size(16, 16);
            this.Load += MarkerForm_Load;
        }

        private void MarkerForm_Load(object? sender, EventArgs e)
        {
            this.Enabled = false;
            this.Size = new Size(16, 16);
            Debug.WriteLine($"MarkerForm cli-w: {this.ClientSize.Width}, cli-h: {this.ClientSize.Height}");
            Debug.WriteLine($"MarkerForm wnd-w: {this.Size.Width}, wnd-h: {this.Size.Height}");
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x0084;
            const int HTCAPTION = 0x02;

            if (m.Msg == WM_NCHITTEST)
            {
                m.Result = (IntPtr)HTCAPTION;
                return;
            }
            base.WndProc(ref m);
        }
    }
}
