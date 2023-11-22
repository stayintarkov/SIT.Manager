using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SIT.Manager.Classes
{
    public static class Win32
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AttachConsole(uint dwProcessId);

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate HandlerRoutine, bool Add);
        // Delegate type to be used as the Handler Routine for SCCH
        delegate Boolean ConsoleCtrlDelegate(CtrlTypes CtrlType);

        // Enumerated type for the control messages sent to the handler routine
        enum CtrlTypes : uint
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GenerateConsoleCtrlEvent(CtrlTypes dwCtrlEvent, uint dwProcessGroupId);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool FreeConsole();

        public static void CloseConsoleProgram(Process proc)
        {
            if (AttachConsole((uint)proc.Id))
            {
                //Disable Ctrl-C handling for our program
                SetConsoleCtrlHandler(null, true);
                GenerateConsoleCtrlEvent(CtrlTypes.CTRL_C_EVENT, 0);

                //Must wait here. If we don't and re-enable Ctrl-C handling below too fast, we might terminate ourselves.
                proc.WaitForExit();

                FreeConsole();

                //Re-enable Ctrl-C handling or any subsequently started programs will inherit the disabled state.
                SetConsoleCtrlHandler(null, false);
            }
        }
    }
}
