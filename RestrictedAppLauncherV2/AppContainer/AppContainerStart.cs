using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestrictedAppLauncherV2.AppContainer
{
    using System;
    using System.Runtime.InteropServices;

    public class AppContainerStart
    {
        #region DLL imports
        //// Import necessary Windows API functions
        //[DllImport("kernel32.dll", SetLastError = true)]
        //private static extern bool CreateProcess(
        //    string lpApplicationName,
        //    string lpCommandLine,
        //    ref SECURITY_ATTRIBUTES lpProcessAttributes,
        //    ref SECURITY_ATTRIBUTES lpThreadAttributes,
        //    bool bInheritHandles,
        //    uint dwCreationFlags,
        //    IntPtr lpEnvironment,
        //    string lpCurrentDirectory,
        //    ref STARTUPINFOEX lpStartupInfoEx,
        //    out PROCESS_INFORMATION lpProcessInformation);

        //[DllImport("kernel32.dll", SetLastError = true)]
        //private static extern bool InitializeProcThreadAttributeList(
        //    IntPtr lpAttributeList,
        //    int dwAttributeCount,
        //    int dwFlags,
        //    ref IntPtr lpSize);

        //[DllImport("kernel32.dll", SetLastError = true)]
        //private static extern bool UpdateProcThreadAttribute(
        //    IntPtr lpAttributeList,
        //    uint dwFlags,
        //    IntPtr attribute,
        //    IntPtr lpValue,
        //    IntPtr cbSize,
        //    IntPtr lpPreviousValue,
        //    IntPtr lpReturnSize);

        //[DllImport("kernel32.dll", SetLastError = true)]
        //private static extern void DeleteProcThreadAttributeList(IntPtr lpAttributeList);

        //[DllImport("kernel32.dll", SetLastError = true)]
        //private static extern bool CloseHandle(IntPtr hObject);

        //[DllImport("advapi32.dll", SetLastError = true)]
        //private static extern bool ConvertStringSidToSid(string StringSid, out IntPtr Sid);

        //[StructLayout(LayoutKind.Sequential)]
        //private struct PROCESS_INFORMATION
        //{
        //    public IntPtr hProcess;
        //    public IntPtr hThread;
        //    public uint dwProcessId;
        //    public uint dwThreadId;
        //}

        //[StructLayout(LayoutKind.Sequential)]
        //private struct SECURITY_ATTRIBUTES
        //{
        //    public int nLength;
        //    public IntPtr lpSecurityDescriptor;
        //    public bool bInheritHandle;
        //}

        //[StructLayout(LayoutKind.Sequential)]
        //private struct STARTUPINFO
        //{
        //    public int cb;
        //    public string lpReserved;
        //    public string lpDesktop;
        //    public string lpTitle;
        //    public int dwX;
        //    public int dwY;
        //    public int dwXSize;
        //    public int dwYSize;
        //    public int dwXCountChars;
        //    public int dwYCountChars;
        //    public int dwFillAttribute;
        //    public int dwFlags;
        //    public short wShowWindow;
        //    public short cbReserved2;
        //    public IntPtr lpReserved2;
        //    public IntPtr hStdInput;
        //    public IntPtr hStdOutput;
        //    public IntPtr hStdError;
        //}

        //[StructLayout(LayoutKind.Sequential)]
        //private struct STARTUPINFOEX
        //{
        //    public STARTUPINFO StartupInfo;
        //    public IntPtr lpAttributeList;
        //}
        #endregion

        // Import necessary Windows API functions
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CreateProcess(
            string lpApplicationName,
            string lpCommandLine,
            ref SECURITY_ATTRIBUTES lpProcessAttributes,
            ref SECURITY_ATTRIBUTES lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFOEX lpStartupInfoEx,
            out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool InitializeProcThreadAttributeList(
            IntPtr lpAttributeList,
            int dwAttributeCount,
            int dwFlags,
            ref IntPtr lpSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool UpdateProcThreadAttribute(
            IntPtr lpAttributeList,
            uint dwFlags,
            IntPtr attribute,
            IntPtr lpValue,
            IntPtr cbSize,
            IntPtr lpPreviousValue,
            IntPtr lpReturnSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern void DeleteProcThreadAttributeList(IntPtr lpAttributeList);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool ConvertStringSidToSid(string StringSid, out IntPtr Sid);

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct STARTUPINFO
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
        private struct STARTUPINFOEX
        {
            public STARTUPINFO StartupInfo;
            public IntPtr lpAttributeList;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SECURITY_CAPABILITIES
        {
            public IntPtr AppContainerSid;
            public IntPtr Capabilities;
            public uint CapabilityCount;
            public uint Reserved;
        }

        private const uint EXTENDED_STARTUPINFO_PRESENT = 0x00080000;
        private const int PROC_THREAD_ATTRIBUTE_SECURITY_CAPABILITIES = 0x0002000B;

        public static bool StartProcessInAppContainer(string appContainerSidString, string applicationPath)
        {
            Console.WriteLine($"launching with SID: {appContainerSidString}");
            IntPtr appContainerSid;
            if (!ConvertStringSidToSid(appContainerSidString, out appContainerSid))
            {
                Console.WriteLine($"Error converting SID: {Marshal.GetLastWin32Error()}");
                return false;
            }

            // Setup the security capabilities
            SECURITY_CAPABILITIES securityCapabilities = new SECURITY_CAPABILITIES
            {
                AppContainerSid = appContainerSid,
                Capabilities = IntPtr.Zero,
                CapabilityCount = 0,
                Reserved = 0
            };

            // Initialize startup info
            STARTUPINFOEX startupInfoEx = new STARTUPINFOEX();
            startupInfoEx.StartupInfo.cb = Marshal.SizeOf(startupInfoEx);

            IntPtr lpSize = IntPtr.Zero;
            InitializeProcThreadAttributeList(IntPtr.Zero, 1, 0, ref lpSize);
            startupInfoEx.lpAttributeList = Marshal.AllocHGlobal(lpSize);
            InitializeProcThreadAttributeList(startupInfoEx.lpAttributeList, 1, 0, ref lpSize);

            // Update the attribute list to include the AppContainer SID
            IntPtr securityCapabilitiesPtr = Marshal.AllocHGlobal(Marshal.SizeOf(securityCapabilities));
            Marshal.StructureToPtr(securityCapabilities, securityCapabilitiesPtr, false);
            UpdateProcThreadAttribute(startupInfoEx.lpAttributeList, 0, (IntPtr)PROC_THREAD_ATTRIBUTE_SECURITY_CAPABILITIES, securityCapabilitiesPtr, (IntPtr)Marshal.SizeOf(securityCapabilities), IntPtr.Zero, IntPtr.Zero);

            // Setup process and thread security attributes
            SECURITY_ATTRIBUTES processAttributes = new SECURITY_ATTRIBUTES();
            SECURITY_ATTRIBUTES threadAttributes = new SECURITY_ATTRIBUTES();

            // Start the process
            PROCESS_INFORMATION processInfo;
            bool result = CreateProcess(
                null,
                applicationPath,
                ref processAttributes,
                ref threadAttributes,
                false,
                EXTENDED_STARTUPINFO_PRESENT,
                IntPtr.Zero,
                null,
                ref startupInfoEx,
                out processInfo);

            if (result)
            {
                Console.WriteLine($"Process launched successfully. PID: {processInfo.dwProcessId}");
            }
            else
            {
                Console.WriteLine($"Failed to launch process. Error: {Marshal.GetLastWin32Error()}");
            }

            // Clean up
            DeleteProcThreadAttributeList(startupInfoEx.lpAttributeList);
            Marshal.FreeHGlobal(startupInfoEx.lpAttributeList);
            Marshal.FreeHGlobal(securityCapabilitiesPtr);
            CloseHandle(processInfo.hProcess);
            CloseHandle(processInfo.hThread);

            return result;
        }

        //private const uint EXTENDED_STARTUPINFO_PRESENT = 0x00080000;

        ///// <summary>
        ///// Starts a process in the specified AppContainer using the provided AppContainer SID and application path.
        ///// </summary>
        ///// <param name="appContainerSidString">The SID of the AppContainer in string format.</param>
        ///// <param name="applicationPath">The full path of the application to launch.</param>
        ///// <returns>Returns true if the process was started successfully, otherwise false.</returns>
        //public static bool StartProcessInAppContainer(string appContainerSidString, string applicationPath)
        //{
        //    const int PROC_THREAD_ATTRIBUTE_SECURITY_CAPABILITIES = 0x0002000B;

        //    IntPtr appContainerSid = ConvertStringSidToSid(appContainerSidString);

        //    // Create attributes for the process startup
        //    STARTUPINFOEX startupInfoEx = new STARTUPINFOEX();
        //    startupInfoEx.StartupInfo.cb = Marshal.SizeOf(typeof(STARTUPINFOEX));

        //    // Get the size of the attribute list for the AppContainer SID
        //    IntPtr lpSize = IntPtr.Zero;
        //    InitializeProcThreadAttributeList(IntPtr.Zero, 1, 0, ref lpSize);

        //    // Allocate the attribute list
        //    startupInfoEx.lpAttributeList = Marshal.AllocHGlobal(lpSize);
        //    InitializeProcThreadAttributeList(startupInfoEx.lpAttributeList, 1, 0, ref lpSize);

        //    // Update the attribute list to include the AppContainer SID
        //    IntPtr appContainerSidPtr = Marshal.AllocHGlobal(Marshal.SizeOf(appContainerSid));
        //    Marshal.StructureToPtr(appContainerSid, appContainerSidPtr, false);
        //    UpdateProcThreadAttribute(startupInfoEx.lpAttributeList, 0, (IntPtr)PROC_THREAD_ATTRIBUTE_SECURITY_CAPABILITIES, appContainerSidPtr, (IntPtr)Marshal.SizeOf(appContainerSid), IntPtr.Zero, IntPtr.Zero);

        //    // Security attributes
        //    SECURITY_ATTRIBUTES processAttributes = new SECURITY_ATTRIBUTES();
        //    SECURITY_ATTRIBUTES threadAttributes = new SECURITY_ATTRIBUTES();

        //    // Create the process in the AppContainer
        //    PROCESS_INFORMATION processInfo;
        //    bool result = CreateProcess(
        //        null,
        //        applicationPath,
        //        ref processAttributes,
        //        ref threadAttributes,
        //        false,
        //        EXTENDED_STARTUPINFO_PRESENT,
        //        IntPtr.Zero,
        //        null,
        //        ref startupInfoEx,
        //        out processInfo);

        //    if (result)
        //    {
        //        Console.WriteLine($"Process launched successfully. PID: {processInfo.dwProcessId}");
        //    }
        //    else
        //    {
        //        Console.WriteLine($"Failed to launch process. Error: {Marshal.GetLastWin32Error()}");
        //    }

        //    // Clean up
        //    DeleteProcThreadAttributeList(startupInfoEx.lpAttributeList);
        //    Marshal.FreeHGlobal(startupInfoEx.lpAttributeList);
        //    Marshal.FreeHGlobal(appContainerSidPtr);
        //    CloseHandle(processInfo.hProcess);
        //    CloseHandle(processInfo.hThread);

        //    return result;
        //}

        /// <summary>
        /// Converts a string SID to a SID pointer.
        /// </summary>
        /// <param name="stringSid">The SID in string format (e.g., "S-1-15-2-1").</param>
        /// <returns>A pointer to the SID structure.</returns>
        private static IntPtr ConvertStringSidToSid(string stringSid)
        {
            if (ConvertStringSidToSid(stringSid, out IntPtr sid))
            {
                return sid;
            }
            else
            {
                throw new Exception($"Error converting SID. Error: {Marshal.GetLastWin32Error()}");
            }
        }
    }

}
