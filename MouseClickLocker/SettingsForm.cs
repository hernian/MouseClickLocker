using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.ApplicationModel.VoiceCommands;

namespace MouseClickLocker
{
    public partial class SettingsForm : Form
    {
        private int _initialXOffset;
        private int _initialYOffset;

        public SettingsForm(MouseClickLockerSettings settings)
        {
            InitializeComponent();

            _initialXOffset = settings.MarkerXOffset;
            _initialYOffset = settings.MarkerYOffset;

            trackBarMarkerXOffset.Value = settings.MarkerXOffset;
            trackBarMarkerYOffset.Value = settings.MarkerYOffset;
            trackBarClickLockDelayMS.Value = settings.ClickLockDelayMS;
        }

        public MouseClickLockerSettings GetSettings()
        {
            var settings = new MouseClickLockerSettings()
            {
                MarkerXOffset = trackBarMarkerXOffset.Value,
                MarkerYOffset = trackBarMarkerYOffset.Value,
                ClickLockDelayMS = trackBarClickLockDelayMS.Value,
            };
            return settings;
        }

        private void PreviewMarkerOffset()
        {
            var xOffset = trackBarMarkerXOffset.Value;
            var yOffset = trackBarMarkerYOffset.Value;
            MouseHookLib.SetMarkerOffset(xOffset, yOffset);
        }

        private void TrackBarMarkerXOffset_ValueChanged(object sender, EventArgs e)
        {
            this.PreviewMarkerOffset();
        }

        private void TrackBarMarkerYOffset_ValueChanged(object sender, EventArgs e)
        {
            this.PreviewMarkerOffset();
        }

        private void TrackBarClickLockDelayMS_ValueChanged(object sender, EventArgs e)
        {
            labelClickDelayMS.Text = $"{trackBarClickLockDelayMS.Value}ミリ秒";
        }
    }
}
