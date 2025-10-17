using Microsoft.VisualBasic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using static MouseClickLocker.Win32Api;

namespace MouseClickLocker
{
    public partial class MainForm : Form
    {
        private const string WM_TASKBARCREATED_STR = "TaskbarCreated";

        private readonly int WM_TASKBARCREATED = RegisterWindowMessage(WM_TASKBARCREATED_STR);

        private MouseClickLockerSettings _settings;
        private bool _isClickLockEnabled = true;
        private MarkerForm? _markerForm;

        public MainForm()
        {
            InitializeComponent();
            _settings = MouseClickLockerSettings.Load();
            isClickLockEnabledToolStripMenuItem.Checked = _isClickLockEnabled;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.Visible = false;
            _markerForm = new MarkerForm();
            _markerForm.CreateControl();

            MouseHookLib.Initialize();
            MouseHookLib.SetClickLockDelayMS(_settings.ClickLockDelayMS);
            MouseHookLib.SetMarkerOffset(_settings.MarkerXOffset, _settings.MarkerYOffset);
            MouseHookLib.AllowMouseMove(_settings.AllowMouseMove, _settings.AllowMouseMoveDistancePx);
            MouseHookLib.EnableClickLock(_isClickLockEnabled);
            MouseHookLib.SetMouseHook(_markerForm.Handle);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            MouseHookLib.UnsetMouseHook();
            base.OnFormClosing(e);
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            aboutToolStripMenuItem.Enabled = false;
            var assembly = Assembly.GetExecutingAssembly();
            var copyrightAttr = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>();
            var copyright = copyrightAttr?.Copyright ?? "Unknown";
            var version = assembly.GetName().Version;
            var versionString = $"Mouse Click Locker\nVersion {version}\n\n(C) 2025 {copyright}";
            MessageBox.Show(this,
                versionString,
                "Mouse Click Lockerについてt",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            aboutToolStripMenuItem.Enabled = true;
        }

        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_markerForm == null)
            {
                return;
            }
            settingsToolStripMenuItem.Enabled = false;
            using var settingsForm = new SettingsForm(_settings);
            _markerForm.Preview = true;
            MouseHookLib.SetMarkerPreview(true);
            var markerXOffset = _settings.MarkerXOffset;
            var markerYOffset = _settings.MarkerYOffset;
            try
            {
                if (settingsForm.ShowDialog(this) == DialogResult.OK)
                {
                    _settings = settingsForm.GetSettings();
                    _settings.Save();
                    MouseHookLib.SetClickLockDelayMS(_settings.ClickLockDelayMS);
                    MouseHookLib.AllowMouseMove(_settings.AllowMouseMove, _settings.AllowMouseMoveDistancePx);
                    // キャンセルしたときに設定前の値に戻せるようにfinallyブロックでmousehooklib.dllに値を反映する
                    markerXOffset = _settings.MarkerXOffset;
                    markerYOffset = _settings.MarkerYOffset;
                }
            }
            finally
            {
                MouseHookLib.SetMarkerOffset(markerXOffset, markerYOffset);
                MouseHookLib.SetMarkerPreview(false);
                _markerForm.Preview = false;
                settingsToolStripMenuItem.Enabled = true;
            }
        }

        private void QuitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void OnWmTaskbarCreated()
        {
            // タスクバーが再作成されたときの処理をここに記述
            Debug.WriteLine("WM_TASKBARCREATED 受信: タスクバーが再作成されました");
            notifyIcon.Visible = false;
            notifyIcon.Visible = true;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_TASKBARCREATED)
            {
                this.OnWmTaskbarCreated();
                return;
            }
            base.WndProc(ref m);
        }

        private void IsClickLockEnabledToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // トグル動作
            _isClickLockEnabled = (isClickLockEnabledToolStripMenuItem.Checked == false);
            MouseHookLib.EnableClickLock(_isClickLockEnabled);
            isClickLockEnabledToolStripMenuItem.Checked = _isClickLockEnabled;
            Debug.WriteLine($"ContextMenuStrip_Closed _isClickLockEnabled: {_isClickLockEnabled}");
        }
    }
}
