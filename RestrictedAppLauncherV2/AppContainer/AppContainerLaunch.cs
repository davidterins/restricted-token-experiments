using System.Runtime.InteropServices;

namespace RestrictedAppLauncherV2.AppContainer
{
    internal class AppContainerLaunch
    {

        // Import necessary functions from the Windows API
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CreateProcess(
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        [StructLayout(LayoutKind.Sequential)]
        public struct STARTUPINFO
        {
            public int cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public int dwX;
            public int dwY;
            public int dwXSize;
            public int dwYSize;
            public int dwXCountChars;
            public int dwYCountChars;
            public int dwFillAttribute;
            public int dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }

        // Constants for process creation
        const uint CREATE_NO_WINDOW = 0x08000000;
        const uint CREATE_NEW_CONSOLE = 0x00000010;

        public static void LaunchProcessInAppContainerV3(string containerName, string applicationPath)
        {
            string appContainerSid = CreateProfile.GetOrCreateAppContainerProfileSid(containerName);

            AppContainerProcessLauncher.LaunchProcessInAppContainer(appContainerSid, applicationPath);
        }

        public static void LaunchProcessInAppContainerV2(string containerName, string applicationPath)
        {
            string appContainerSid = CreateProfile.GetOrCreateAppContainerProfileSid(containerName);

            _ = AppContainerStart.StartProcessInAppContainer(appContainerSid, applicationPath);
        }

        public static void LaunchProcessInAppContainer(string containerName, string applicationPath)
        {
            string appContainerSid = CreateProfile.GetOrCreateAppContainerProfileSid(containerName);

            // Set up the startup info for the new process
            STARTUPINFO startupInfo = new STARTUPINFO();
            startupInfo.cb = Marshal.SizeOf(typeof(STARTUPINFO));
            startupInfo.lpDesktop = "winsta0\\default"; // Set desktop to default

            // Prepare the process information structure
            PROCESS_INFORMATION processInfo;

            // Create the AppContainer process
            uint creationFlags = CREATE_NO_WINDOW | CREATE_NEW_CONSOLE;

            // Add the AppContainer SID to the command line arguments
            string commandLine = $"\"{applicationPath}\""; // Example command line

            if (CreateProcess(
                    null,
                    commandLine,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    false,
                    creationFlags,
                    IntPtr.Zero,
                    null,
                    ref startupInfo,
                    out processInfo))
            {
                Console.WriteLine("Process launched successfully.");
                // Close handles to avoid leaks
                CloseHandle(processInfo.hProcess);
                CloseHandle(processInfo.hThread);
            }
            else
            {
                Console.WriteLine($"Failed to launch process. Error: {Marshal.GetLastWin32Error()}");
            }
        }

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hHandle);
    }

}
