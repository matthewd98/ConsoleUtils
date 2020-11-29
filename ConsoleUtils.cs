using System;
using System.Runtime.InteropServices;

namespace ConsoleUtils
{
    public static class ConsoleUtils
    {
        #region WIN32 Constants

        const uint ENABLE_QUICK_EDIT = 0x0040;

        // STD_INPUT_HANDLE (DWORD): -10 is the standard input device.
        const int STD_INPUT_HANDLE = -10;

        const uint GENERIC_READ = 0x80000000;
        const uint GENERIC_WRITE = 0x40000000;
        const uint FILE_SHARE_READ = 0x00000001;
        const uint OPEN_EXISTING = 3;

        const uint SW_MINIMIZE = 6;

        #endregion

        #region WIN32 Methods

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern IntPtr CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr SecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile
        );

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        #endregion

        public static void CheckAndDisableQuickEditMode()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var consoleHandle = GetStdHandle(STD_INPUT_HANDLE);

                if (!GetConsoleMode(consoleHandle, out var consoleMode))
                {
                    // If STDIN is redirected, then need to get console handle with CreateFile method
                    consoleHandle = CreateFile("CONIN$", GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
                    if (!GetConsoleMode(consoleHandle, out consoleMode))
                    {
                        Console.WriteLine("Unable to get console mode.");
                        return;
                    }
                }

                if ((consoleMode & ENABLE_QUICK_EDIT) == ENABLE_QUICK_EDIT)
                {
                    Console.WriteLine("Quick Edit mode in the console is enabled. Disabling it to prevent random freezing of the console app when waiting for keyboard input.");

                    // Clear the quick edit bit in the mode flags
                    consoleMode &= ~ENABLE_QUICK_EDIT;

                    if (!SetConsoleMode(consoleHandle, consoleMode))
                    {
                        Console.WriteLine("Failed to disable the Quick Edit mode in the console.");
                    }
                }
            }
        }

        public static void MinimizeConsoleWindow()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var consoleWindowHandle = GetConsoleWindow();
                if (consoleWindowHandle != null)
                {
                    ShowWindow(consoleWindowHandle, (int)SW_MINIMIZE);
                }
            }
        }
    }
}
