using Microsoft.VisualBasic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using static MouseClickLocker.Win32Api;

namespace MouseClickLocker
{
    public partial class MainForm : Form
    {
        private MouseClickLockerSettings _settings;
        private bool _isClickLockEnabled = true;
        private MarkerForm _markerForm;
        public MainForm()
        {
            InitializeComponent();

            _settings = MouseClickLockerSettings.Load();
            MouseHookLib.Initialize();
            MouseHookLib.SetClickLockDelayMS(_settings.ClickLockDelayMS);
            MouseHookLib.SetMarkerOffset(_settings.MarkerXOffset, _settings.MarkerYOffset);

            _markerForm = new MarkerForm();

            this.Closing += MainForm_Closing;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.Visible = false;
            _markerForm.CreateControl();

            MouseHookLib.EnableClickLock(_isClickLockEnabled);
            MouseHookLib.SetMouseHook(_markerForm.Handle);

            Debug.WriteLine($"MarkerForm w: {_markerForm.Size.Width}, h: {_markerForm.Size.Height}");
        }

        private async void MainForm_Closing(object? sender, CancelEventArgs e)
        {
            this.Closing -= MainForm_Closing;
            e.Cancel = true;
            // ここで一度呼び出し元へ返る
            await Task.Yield();

            MouseHookLib.UnsetMouseHook();
            _markerForm.Close();
            this.Close();
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
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
        }

        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using var settingsForm = new SettingsForm(_settings);
            _markerForm.Preview = true;
            MouseHookLib.SetMarkerPreview(false);
            try
            {
                if (settingsForm.ShowDialog(this) == DialogResult.OK)
                {
                    _settings = settingsForm.GetSettings();
                    _settings.Save();
                    MouseHookLib.SetClickLockDelayMS(_settings.ClickLockDelayMS);
                    MouseHookLib.SetMarkerOffset(_settings.MarkerXOffset, _settings.MarkerYOffset);
                }
            }
            finally
            {
                MouseHookLib.SetMarkerOffset(_settings.MarkerXOffset, _settings.MarkerYOffset);
                MouseHookLib.SetMarkerPreview(false);
                _markerForm.Preview = false;
            }
        }

        private void QuitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            isClickLockEnabledToolStripMenuItem.Checked = _isClickLockEnabled;
        }

        private void ContextMenuStrip_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            // トグル動作
            _isClickLockEnabled = (isClickLockEnabledToolStripMenuItem.Checked == false);
            MouseHookLib.EnableClickLock(_isClickLockEnabled);
        }
    }
}
