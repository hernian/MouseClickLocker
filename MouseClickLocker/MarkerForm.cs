using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace MouseClickLocker
{
    public partial class MarkerForm : LayeredWindow
    {
        public enum LockState
        {
            None,
            Left,
            Right,
            Both
        }

        private readonly Dictionary<LockState, Bitmap> _lockBitmatDict = new()
        {
            { LockState.None, Properties.Resources.LockImageLeft },
            { LockState.Left, Properties.Resources.LockImageLeft },
            { LockState.Right, Properties.Resources.LockImageRight },
            { LockState.Both, Properties.Resources.LockImageBoth }
        };

        public MarkerForm()
        {
            InitializeComponent();
            this.AutoScaleMode = AutoScaleMode.None;
            this.Enabled = false;
            this.Load += MarkerForm_Load;
        }

        private void MarkerForm_Load(object? sender, EventArgs e)
        {
            var bitmap = _lockBitmatDict[LockState.None];
            this.Size = bitmap.Size;
            this.SetLayeredBitmap(bitmap);
        }

        public void SetLockState(LockState state)
        {
            var bitmap = _lockBitmatDict[state];
            this.SetLayeredBitmap(bitmap);
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
