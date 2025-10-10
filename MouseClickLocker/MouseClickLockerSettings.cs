using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseClickLocker
{
    public class MouseClickLockerSettings
    {
        public static MouseClickLockerSettings Load()
        {
            var settings = new MouseClickLockerSettings()
            {
                MarkerXOffset = Properties.Settings.Default.MarkerXOffset,
                MarkerYOffset = Properties.Settings.Default.MarkerYOffset,
                ClickLockDelayMS = Properties.Settings.Default.ClickLockDelayMS,
            };
            return settings;
        }
        public int MarkerXOffset { get; init; }
        public int MarkerYOffset { get; init; }
        public int ClickLockDelayMS { get; init; }
        public void Save()
        {
            Properties.Settings.Default.MarkerXOffset = this.MarkerXOffset;
            Properties.Settings.Default.MarkerYOffset = this.MarkerYOffset;
            Properties.Settings.Default.ClickLockDelayMS = this.ClickLockDelayMS;
            Properties.Settings.Default.Save();
        }
    }
}
