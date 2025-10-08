using System.Diagnostics;
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
            int r = SetProcessDpiAwareness(ProcessDpiAwareness.Process_Per_Monitor_DPI_Aware);
            Debug.WriteLine($"SetProcessDpiAwareness: {r}");
            int r2 = GetProcessDpiAwareness(Process.GetCurrentProcess().Handle, out int awareness);
            Debug.WriteLine($"GetProcessDpiAwareness: {r2}, awareness: {(ProcessDpiAwareness)awareness}");

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}