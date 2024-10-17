using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestrictedAppLauncherV2.AppContainer
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    class AppContainerProcessLauncher
    {
        internal const uint STANDARD_RIGHTS_REQUIRED = 0x000F0000;
        internal const uint STANDARD_RIGHTS_READ = 0x00020000;
        internal const uint TOKEN_ASSIGN_PRIMARY = 0x0001;
        internal const uint TOKEN_DUPLICATE = 0x0002;
        internal const uint TOKEN_IMPERSONATE = 0x0004;
        internal const uint TOKEN_QUERY = 0x0008;
        internal const uint TOKEN_QUERY_SOURCE = 0x0010;
        internal const uint TOKEN_ADJUST_PRIVILEGES = 0x0020;
        internal const uint TOKEN_ADJUST_GROUPS = 0x0040;
        internal const uint TOKEN_ADJUST_DEFAULT = 0x0080;
        internal const uint TOKEN_ADJUST_SESSIONID = 0x0100;
        internal const uint TOKEN_READ = STANDARD_RIGHTS_READ | TOKEN_QUERY;
        internal const uint TOKEN_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED |
                            TOKEN_ASSIGN_PRIMARY |
                            TOKEN_DUPLICATE |
                            TOKEN_IMPERSONATE |
                            TOKEN_QUERY |
                            TOKEN_QUERY_SOURCE |
                            TOKEN_ADJUST_PRIVILEGES |
                            TOKEN_ADJUST_GROUPS |
                            TOKEN_ADJUST_DEFAULT |
                            TOKEN_ADJUST_SESSIONID;

        internal const uint Token_Perms = TOKEN_DUPLICATE |
            TOKEN_ADJUST_DEFAULT |
            TOKEN_QUERY |
            TOKEN_ASSIGN_PRIMARY |
            TOKEN_ADJUST_PRIVILEGES |
            TOKEN_ADJUST_GROUPS;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hObject);

        // P/Invoke declarations
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CreateProcessAsUser(
            IntPtr hToken,
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            [In] ref STARTUPINFOEX lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr VirtualAlloc(IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool VirtualFree(IntPtr lpAddress, uint dwSize, uint dwFreeType);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool ConvertStringSidToSid(string StringSid, out IntPtr pSid);

        [DllImport("kernel32.dll")]
        public static extern void LocalFree(IntPtr hMem);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool InitializeProcThreadAttributeList(IntPtr lpAttributeList, uint dwAttributeCount, uint dwFlags, ref IntPtr lpSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool UpdateProcThreadAttribute(IntPtr lpAttributeList, uint dwFlags, IntPtr Attribute, IntPtr lpValue, uint cbSize, IntPtr lpPreviousValue, IntPtr lpReturnValue);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool DeleteProcThreadAttributeList(IntPtr lpAttributeList);

        [StructLayout(LayoutKind.Sequential)]
        public struct STARTUPINFOEX
        {
            public int StartupInfoLength;
            public string lpDesktop;
            public string lpTitle;
            public uint dwFlags;
            public ushort wShowWindow;
            public ushort cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
            public IntPtr lpAttributeList; // Pointer to PROC_THREAD_ATTRIBUTE_LIST
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_CAPABILITIES
        {
            public IntPtr AppContainerSid;
            public uint CapabilityCount;
            public IntPtr Capabilities; // Pointer to an array of strings
        }

        const uint EXTENDED_STARTUPINFO_PRESENT = 0x00080000;
        const uint PROC_THREAD_ATTRIBUTE_SECURITY_CAPABILITIES = 0x2000B; // Correct value
        const uint CREATE_NEW_CONSOLE = 0x00000010;

        public static void LaunchProcessInAppContainer(string appContainerSid, string applicationPath)
        {
            IntPtr userToken = IntPtr.Zero;
            IntPtr appContainerSidPtr = IntPtr.Zero;
            IntPtr attributeList = IntPtr.Zero;

            try
            {
                // Get the user token of the foreground process
                IntPtr hWnd = GetForegroundWindow();
                Process foregroundProcess = Process.GetProcessById(Process.GetCurrentProcess().Id);

                if (!OpenProcessToken(foregroundProcess.Handle, Token_Perms, out userToken))
                {
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                }

                // Convert the AppContainer SID string to SID
                if (!ConvertStringSidToSid(appContainerSid, out appContainerSidPtr))
                {
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                }

                // Initialize the PROC_THREAD_ATTRIBUTE_LIST
                IntPtr size = IntPtr.Zero;
                InitializeProcThreadAttributeList(IntPtr.Zero, 1, 0, ref size);
                attributeList = Marshal.AllocHGlobal(size);

                if (!InitializeProcThreadAttributeList(attributeList, 1, 0, ref size))
                {
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                }
                // Update the PROC_THREAD_ATTRIBUTE_LIST with the AppContainer SID

                // Set up the SECURITY_CAPABILITIES structure
                SECURITY_CAPABILITIES sc = new()
                {
                    AppContainerSid = appContainerSidPtr,
                    CapabilityCount = 0, // Set this to the number of capabilities if any
                    Capabilities = IntPtr.Zero // Set if you have capability strings
                };

                int securityCapabilitiesSize = Marshal.SizeOf(typeof(SECURITY_CAPABILITIES));
                IntPtr securityCapabilitiesPtr = Marshal.AllocHGlobal(securityCapabilitiesSize);
                Marshal.StructureToPtr(sc, securityCapabilitiesPtr, false);

                if (!UpdateProcThreadAttribute(attributeList, 0, 
                    (IntPtr)PROC_THREAD_ATTRIBUTE_SECURITY_CAPABILITIES, securityCapabilitiesPtr, (uint)securityCapabilitiesSize,
                    IntPtr.Zero, IntPtr.Zero))
                {
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                }

                // Prepare the STARTUPINFOEX structure
                STARTUPINFOEX si = new STARTUPINFOEX
                {
                    StartupInfoLength = Marshal.SizeOf(typeof(STARTUPINFOEX)),
                    lpDesktop = null, // Use default desktop
                    dwFlags = EXTENDED_STARTUPINFO_PRESENT,
                    lpAttributeList = attributeList
                };

                PROCESS_INFORMATION pi;



                // Create the process as a user in the AppContainer
                if (!CreateProcessAsUser(userToken, applicationPath, null, IntPtr.Zero, IntPtr.Zero, false,
                    CREATE_NEW_CONSOLE | EXTENDED_STARTUPINFO_PRESENT, IntPtr.Zero, null, ref si, out pi))
                {
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                }

                Console.WriteLine("Process started successfully in AppContainer.");
            }
            finally
            {
                // Clean up handles
                if (userToken != IntPtr.Zero) CloseHandle(userToken);
                if (appContainerSidPtr != IntPtr.Zero) LocalFree(appContainerSidPtr);
                if (attributeList != IntPtr.Zero) DeleteProcThreadAttributeList(attributeList);
            }
        }
    }
}
