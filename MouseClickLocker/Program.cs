using System.Diagnostics;
using System.Threading; // 追加
using static MouseClickLocker.Win32Api;

namespace MouseClickLocker
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // 多重起動抑止用ミューテックス
            bool createdNew;
            using var mutex = new Mutex(true, "MouseClickLocker_Mutex", out createdNew);
            if (!createdNew)
            {
                // 既に起動している場合は終了
                MessageBox.Show("Mouse Click Lockerは既に起動しています。", "多重起動防止", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int r = SetProcessDpiAwareness(ProcessDpiAwareness.Process_Per_Monitor_DPI_Aware);
            Debug.WriteLine($"SetProcessDpiAwareness: {r}");
            int r2 = GetProcessDpiAwareness(Process.GetCurrentProcess().Handle, out int awareness);
            Debug.WriteLine($"GetProcessDpiAwareness: {r2}, awareness: {(ProcessDpiAwareness)awareness}");

            // 実行優先度を「高」に設定、マウスイベントの処理が他のプロセスに邪魔されにくくなる
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}