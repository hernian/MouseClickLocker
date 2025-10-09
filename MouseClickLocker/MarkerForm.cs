using System;
using System.Diagnostics;
using System.Windows.Forms;
using static MouseClickLocker.Win32Api;
using static MouseClickLocker.MouseHookLib;
using System.Media;

namespace MouseClickLocker
{
    public partial class MarkerForm : LayeredWindow
    {
        private readonly Bitmap _lockImageLeft = Properties.Resources.LockImageLeft;
        private readonly Bitmap _lockImageRight = Properties.Resources.LockImageRight;
        private readonly Bitmap _lockImageBoth = Properties.Resources.LockImageBoth;
        private Bitmap _layeredBitmap;
        private bool _isLeftButtonLocked = false;
        private bool _isRightButtonLocked = false;
        private readonly SoundPlayer _soundPlayer;

        public MarkerForm()
        {
            InitializeComponent();
            this.AutoScaleMode = AutoScaleMode.None;
            this.Enabled = false;
            this.Visible = false;
            this.Load += MarkerForm_Load;

            _layeredBitmap = _lockImageLeft;
            _soundPlayer = new SoundPlayer(Properties.Resources.ClickLockONSound);
        }

        private void MarkerForm_Load(object? sender, EventArgs e)
        {
            this.Size = _layeredBitmap.Size;
            this.SetLayeredBitmap(_layeredBitmap);
        }

        private Bitmap? GetLockStateBitmap()
        {
            if (this._isLeftButtonLocked && this._isRightButtonLocked)
            {
                return _lockImageBoth;
            }
            if (this._isLeftButtonLocked)
            {
                return _lockImageLeft;
            }
            if (this._isRightButtonLocked)
            {
                return _lockImageRight;
            }
            return null;
        }

        private void UpdateLayeredBitmap()
        {
            var bitmap = this.GetLockStateBitmap();
            if (bitmap != null)
            {
                if (_layeredBitmap != bitmap)
                {
                    _layeredBitmap = bitmap;
                    this.SetLayeredBitmap(_layeredBitmap);
                }
                this.Visible = true;
            }
            else
            {
                this.Visible = false;
            }
        }

        private void OnNotifyLockState(ref Message m)
        {
            switch (m.WParam)
            { 
                case LOCK_STATE_LEFT:
                    this._isLeftButtonLocked = (m.LParam == 1);
                    break;
                case LOCK_STATE_RIGHT:
                    this._isRightButtonLocked = (m.LParam == 1);
                    break;
            }
            if (m.LParam == 1)
            {
                _soundPlayer.Play();
            }
            this.UpdateLayeredBitmap();
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_NCHITTEST)
            {
                m.Result = (IntPtr)HTCAPTION;
                return;
            }
            if (m.Msg == WM_NOTIFY_LOCK_STATE)
            {
                this.OnNotifyLockState(ref m);
                return;
            }
            base.WndProc(ref m);
        }
    }
}
