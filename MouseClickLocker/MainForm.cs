using Microsoft.VisualBasic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using static MouseClickLocker.MouseHookLib;
using static MouseClickLocker.Win32Api;

namespace MouseClickLocker
{
    public partial class MainForm : Form
    {
        private MouseClickLockerSettings _settings;
        private MarkerForm _markerForm;
        public MainForm()
        {
            InitializeComponent();

            _settings = MouseClickLockerSettings.Load();
            MouseHookLib.Initialize();
            MouseHookLib.SetClickLockDelay(_settings.ClickLockDelayMS);
            MouseHookLib.SetMarkerOffset(_settings.MarkerXOffset, _settings.MarkerYOffset);

            _markerForm = new MarkerForm();

            this.Closing += MainForm_Closing;
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            GetProcessDpiAwareness(Process.GetCurrentProcess().Handle, out int awareness);
            Debug.WriteLine($"Process DPI Awareness: {(ProcessDpiAwareness)awareness}");

            _markerForm.CreateControl();

            ActivateClickLock(true);
            SetMouseHook(this.Handle, _markerForm.Handle);

            Debug.WriteLine($"MarkerForm w: {_markerForm.Size.Width}, h: {_markerForm.Size.Height}");
        }

        private async void MainForm_Closing(object? sender, CancelEventArgs e)
        {
            this.Closing -= MainForm_Closing;
            e.Cancel = true;
            // Ç±Ç±Ç≈àÍìxåƒÇ—èoÇµå≥Ç÷ï‘ÇÈ
            await Task.Yield();

            UnsetMouseHook();
            _markerForm.Close();
            this.Close();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            foreach (var s in Screen.AllScreens)
            {
                Debug.WriteLine($"Device Name: {s.DeviceName}\n" +
                                $"Bounds: {s.Bounds}\n" +
                                $"Type: {(s.Primary ? "Primary" : "Secondary")}\n" +
                                $"Working Area: {s.WorkingArea}\n" +
                                $"Bits Per Pixel: {s.BitsPerPixel}");
                var pt = new Point(s.Bounds.X + 1, s.Bounds.Y + 1);
                var hMonitor = MonitorFromPoint(pt, MONITOR_DEFAULTTONEAREST);
                int r = GetDpiForMonitor(hMonitor, MDT_EFFECTIVE_DPI, out uint dpiX, out uint dpiY);
                Debug.WriteLine($"GetDpiForMonitor: {r}, dpiX: {dpiX}, dpiY: {dpiY}");
                Debug.WriteLine("");
            }
        }

        private void MainForm_Move(object sender, EventArgs e)
        {
            var b = this.Bounds;
            Debug.WriteLine($"Window Rect: x: {b.X}, y: {b.Y}, w: {b.Width}, h: {b.Height}");
            Debug.WriteLine($"Window DPI: {this.DeviceDpi}");
            var s = Screen.FromControl(this);
            Debug.WriteLine($"Screen Name: {s.DeviceName}");
            Debug.WriteLine($"Screen Bounds: x: {s.Bounds.X}, y: {s.Bounds.Y}, w: {s.Bounds.Width}, h: {s.Bounds.Height}");
            Debug.WriteLine("");
        }


        private void OnNotifyMouseEvent(int msg, Point pt)
        {
            Debug.WriteLine($"OnNotifyMouseEvent msg: {msg:x}, x: {pt.X}, y: {pt.Y}");
            var hMonitor = MonitorFromPoint(pt, MONITOR_DEFAULTTONEAREST);
            Debug.WriteLine($"hMonitor: {hMonitor:x08}");
            int r = GetDpiForMonitor(hMonitor, MDT_EFFECTIVE_DPI, out uint dpiX, out uint dpiY);
            Debug.WriteLine($"r: {r}, dpiX: {dpiX}, dpiY: {dpiY}");
            Debug.WriteLine("");
            _markerForm.Location = pt;
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
                "Mouse Click LockerÇ…Ç¬Ç¢Çƒt",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using var settingsForm = new SettingsForm(_settings);
            _markerForm.OffsetPreview = true;
            MouseHookLib.SetMarkerOffsetPreview(false);
            try
            {
                if (settingsForm.ShowDialog(this) == DialogResult.OK)
                {
                    _settings = settingsForm.GetSettings();
                    _settings.Save();
                    MouseHookLib.SetClickLockDelay(_settings.ClickLockDelayMS);
                    MouseHookLib.SetMarkerOffset(_settings.MarkerXOffset, _settings.MarkerYOffset);
                }
            }
            finally
            {
                MouseHookLib.SetMarkerOffsetPreview(false);
                _markerForm.OffsetPreview = false;
            }
        }

        private void QuitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {

        }
    }
}
