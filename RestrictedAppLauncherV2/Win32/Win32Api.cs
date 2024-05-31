using System.Runtime.InteropServices;
using System.Text;

namespace RestrictedAppLauncherV2.Win32
{

    internal static class Win32Api
    {
        public const uint TOKEN_QUERY = 0x0008;

        [StructLayout(LayoutKind.Sequential)]
        public struct TOKEN_GROUPS_AND_PRIVILEGES
        {
            public uint SidCount;
            public uint SidLength;
            public IntPtr Sids;
            public uint RestrictedSidCount;
            public uint RestrictedSidLength;
            public IntPtr RestrictedSids;
            public uint PrivilegeCount;
            public uint PrivilegeLength;
            public IntPtr Privileges;
            public LUID AuthenticationID;
        }

        [DllImport("advapi32", CharSet = CharSet.Auto)]
        public static extern bool ConvertSidToStringSid
        (
            IntPtr psid,
            [In, Out, MarshalAs(UnmanagedType.LPTStr)] ref string pStringSid
        );

        [StructLayout(LayoutKind.Sequential)]
        public struct LUID
        {
            public uint LowPart;
            public int HighPart;
        }
        public enum TOKEN_INFORMATION_CLASS
        {
            TokenUser = 1,
            TokenGroups,
            TokenPrivileges,
            TokenOwner,
            TokenPrimaryGroup,
            TokenDefaultDacl,
            TokenSource,
            TokenType,
            TokenImpersonationLevel,
            TokenStatistics,
            TokenRestrictedSids,
            TokenSessionId,
            TokenGroupsAndPrivileges,
            TokenSessionReference,
            TokenSandBoxInert,
            TokenAuditPolicy,
            TokenOrigin
        }
        [DllImport("Advapi32.dll", EntryPoint = "LookupPrivilegeNameW", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool LookupPrivilegeName(string SystemName, ref LUID LUID, StringBuilder PrivilegeName, ref uint NameLength);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);

        [DllImport("kernel32.dll", EntryPoint = "GetCurrentProcess")]
        public static extern IntPtr GetCurrentProcessPtr();

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool GetTokenInformation(IntPtr TokenHandle,
            TOKEN_INFORMATION_CLASS TokenInformationClass,
            IntPtr TokenInformation,
            int TokenInformationLength,
            out int ReturnLength);

    }
}
