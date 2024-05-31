using Microsoft.Win32.SafeHandles;
using RestrictedAppLauncherV2.Win32.Native;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static RestrictedAppLauncherV2.Win32.Native.ProcessApi;

namespace RestrictedAppLauncherV2.Win32
{
    public static class ProcessExtensions
    {
        internal static SafeProcessHandle CreateProcessAsUser(SafeTokenHandle newToken, string appToRun, string args, string startupFolder, bool newWindow, bool hidden)
        {
            var startupInfo = new STARTUPINFO();
            
            if (newWindow)
            {
                startupInfo.dwFlags = 0x00000001; // STARTF_USESHOWWINDOW
                startupInfo.wShowWindow = (short)(hidden ? 0 : 1);
            }

            startupInfo.cb = Marshal.SizeOf(startupInfo);

            uint dwCreationFlags = newWindow ? (uint)CreateProcessFlags.CREATE_NEW_CONSOLE : 0;

            if (!TokensApi.CreateProcessAsUser(
                    newToken,
                    appToRun.UnQuote(), $"{appToRun} {args}",
                    IntPtr.Zero,
                    IntPtr.Zero,
                    false,
                    dwCreationFlags,
                    IntPtr.Zero,
                    startupFolder,
                    ref startupInfo,
                    out PROCESS_INFORMATION processInfo))
            {
                throw new Win32Exception();
            }

            Process.GetProcessById(processInfo.dwProcessId);

            CloseHandle(processInfo.hThread);
            return new SafeProcessHandle(processInfo.hProcess, true);
        }
    }
}
