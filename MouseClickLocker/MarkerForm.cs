using System;
using System.Diagnostics;
using System.Windows.Forms;
using static MouseClickLocker.Win32Api;
using System.Media;
    
namespace MouseClickLocker
{
    using LockState = MouseHookLib.LockState;

    public partial class MarkerForm : LayeredWindow
    {
        private readonly Bitmap _lockImageLeft = Properties.Resources.LockImageLeft;
        private readonly Bitmap _lockImageRight = Properties.Resources.LockImageRight;
        private readonly Bitmap _lockImageBoth = Properties.Resources.LockImageBoth;
        private readonly Bitmap _lockImageNone = Properties.Resources.LockImageNone;
        private readonly Bitmap _lockImageSuppressed = Properties.Resources.LockImageSuppressed;

        private readonly SoundPlayer _lockActivatedSoundPlayer = new(Properties.Resources.ClickLockActivatedSound);
        private readonly SoundPlayer _lockSuppressedSoundPlayer = new(Properties.Resources.ClickLockSuppressedSound);
        private readonly SoundPlayer _lockDeactivatedSoundPlayer = new(Properties.Resources.ClickLockDeactivatedSound);

        private Bitmap _layeredBitmap;
        private LockState _leftLockState = LockState.DEACTIVATED;
        private LockState _rightLockState = LockState.DEACTIVATED;
        private bool _preview = false;
        private readonly System.Windows.Forms.Timer _timer;

        public MarkerForm()
        {
            InitializeComponent();
            this.AutoScaleMode = AutoScaleMode.None;
            this.Enabled = false;
            this.Visible = false;
            this.Load += MarkerForm_Load;

            _layeredBitmap = _lockImageNone;
            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 2000;
            _timer.Tick += Timer_Tick;
        }
        private void Timer_Tick(object? sender, EventArgs e)
        {
            _timer.Stop();
            if ((_leftLockState == LockState.SUPPRESSED) || (_rightLockState == LockState.SUPPRESSED))
            {
                this.Visible = false;
            }
        }

        public bool Preview
        {
            get
            {
                return _preview;
            }
            set
            {
                _preview = value;
                if (_preview)
                {
                    this.Visible = true;
                }
                else if ((_leftLockState != LockState.DEACTIVATED) || (_rightLockState != LockState.DEACTIVATED))
                {
                    this.Visible = true;
                }
                else
                {
                    this.Visible = false;
                }
            }
        }

        private void MarkerForm_Load(object? sender, EventArgs e)
        {
            this.Size = _layeredBitmap.Size;
            this.SetLayeredBitmap(_layeredBitmap);
        }

        private Bitmap GetLockStateBitmap()
        {
            if ((_leftLockState == LockState.SUPPRESSED) || (_rightLockState == LockState.SUPPRESSED))
            {
                return _lockImageSuppressed;
            }
            if ((_leftLockState == LockState.ACTIVATED) && (_rightLockState == LockState.ACTIVATED))
            {
                return _lockImageBoth;
            }
            if (_leftLockState == LockState.ACTIVATED)
            {
                return _lockImageLeft;
            }
            if (_rightLockState == LockState.ACTIVATED)
            {
                return _lockImageRight;
            }
            return _lockImageNone;
        }

        private void UpdateLayeredBitmap()
        {
        }

        private bool IsClickLockDeactivated(LockState prev, LockState cur)
        {
            if ((prev == LockState.ACTIVATED) && (cur == LockState.DEACTIVATED))
            {
                return true;
            }
            return false;
        }

        private void OnNotifyLockState(ref Message m)
        {
            Debug.WriteLine("[MarkerForm]OnNotifyLockState");
            var leftLockStatePrev = _leftLockState;
            var rightLockStatePrev = _rightLockState;
            var lockType = (int)m.WParam;
            var lockState = (LockState)m.LParam;
            switch (lockType)
            { 
                case MouseHookLib.LOCKTYPE_LEFT:
                    _leftLockState = lockState;
                    break;
                case MouseHookLib.LOCKTYPE_RIGHT:
                    _rightLockState = lockState;
                    break;
            }
            switch (lockState)
            {
                case LockState.ACTIVATED:
                    _lockActivatedSoundPlayer.Play();
                    break;
                case LockState.SUPPRESSED:
                    _lockSuppressedSoundPlayer.Play();
                    break;
                default:
                    break;
            }

            var bitmap = this.GetLockStateBitmap();
            if (_layeredBitmap != bitmap)
            {
                _layeredBitmap = bitmap;
                this.SetLayeredBitmap(_layeredBitmap);
            }

            Debug.WriteLine($"[MarkerForm]_leftLockState: {_leftLockState}, _right: {_rightLockState}, _preview: {_preview}");
            this.Visible = (_preview || (_leftLockState != LockState.DEACTIVATED) || (_rightLockState != LockState.DEACTIVATED));
            _timer.Stop();
            if (_leftLockState == LockState.SUPPRESSED || _rightLockState == LockState.SUPPRESSED)
            {
                _timer.Start();
            }
            else if (IsClickLockDeactivated(leftLockStatePrev, _leftLockState) || IsClickLockDeactivated(rightLockStatePrev, _rightLockState))
            {
                _lockDeactivatedSoundPlayer.Play();
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_NCHITTEST)
            {
                m.Result = (IntPtr)HTCAPTION;
                return;
            }
            if (m.Msg == MouseHookLib.WM_NOTIFY_LOCK_STATE)
            {
                this.OnNotifyLockState(ref m);
                return;
            }
            base.WndProc(ref m);
        }
    }
}
