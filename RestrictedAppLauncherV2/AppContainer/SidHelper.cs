using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace RestrictedAppLauncherV2.AppContainer
{
    internal class SidHelper
    {
        #region DLL imports
        [DllImport("userenv.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int DeriveAppContainerSidFromAppContainerName(
        string pszAppContainerName,
        out IntPtr ppsidAppContainerSid);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern int GetNamedSecurityInfo(
        string pObjectName,
        int ObjectType,
        int SecurityInfo,
        out IntPtr pSidOwner,
        IntPtr pSidGroup,
        IntPtr pDacl,
        IntPtr pSacl,
        out IntPtr ppSecurityDescriptor);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool LookupAccountName(
        string lpSystemName,
        string lpAccountName,
        IntPtr Sid,
        ref int cbSid,
        IntPtr ReferencedDomainName,
        ref int cchReferencedDomainName,
        out int peUse);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool ConvertSidToStringSid(
            IntPtr Sid,
            out string StringSid);

        [DllImport("kernel32.dll")]
        private static extern void LocalFree(IntPtr hMem);
        #endregion

        public static string TryGetSidFromAppContainerV1(string appContainerName)
        {
            IntPtr sidPtr = IntPtr.Zero;

            try
            {
                // Allocate a buffer for the SID
                int sidSize = 0;
                int domainNameSize = 0;
                int use;

                // First call to get the buffer size
                LookupAccountName(null, appContainerName, sidPtr, ref sidSize, IntPtr.Zero, ref domainNameSize, out use);
                sidPtr = Marshal.AllocHGlobal(sidSize); // Allocate memory for the SID

                // Now call it again to retrieve the SID
                if (LookupAccountName(null, appContainerName, sidPtr, ref sidSize, IntPtr.Zero, ref domainNameSize, out use))
                {
                    // Successfully retrieved SID, convert to string
                    ConvertSidToStringSid(sidPtr, out string stringSid);
                    Console.WriteLine($"AppContainer SID for '{appContainerName}': {stringSid}");
                    return stringSid;
                }
                else
                {
                    Console.WriteLine($"Failed to lookup account name. Error: {Marshal.GetLastWin32Error()}");
                    return "";
                }
            }
            finally
            {
                // Free the allocated SID memory
                if (sidPtr != IntPtr.Zero)
                {
                    LocalFree(sidPtr);
                }
            }
        }

        public static string TryGetSidFromAppContainerV2(string appContainerName)
        {
            const int SE_FILE_OBJECT = 1;
            const int SE_KERNEL_OBJECT = 5;
            const int OWNER_SECURITY_INFORMATION = 0x00000001;

            // Retrieve the SID for the AppContainer
            IntPtr sidOwner;
            IntPtr securityDescriptor;
            var result = GetNamedSecurityInfo(appContainerName, SE_KERNEL_OBJECT, OWNER_SECURITY_INFORMATION, out sidOwner, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, out securityDescriptor);

            try
            {
                if (result == 0)
                {
                    // Convert the SID to a SecurityIdentifier for easier manipulation
                    SecurityIdentifier sid = new SecurityIdentifier(sidOwner);
                    Console.WriteLine($"SID: {sid.Value}");
                    return sid.Value;
                }
                else
                {
                    Console.WriteLine($"Failed to get SID. Error: {result}");
                    return "";
                }
            }
            finally
            {
                // Clean up
                if (securityDescriptor != IntPtr.Zero)
                {
                    LocalFree(securityDescriptor);
                }
                if (sidOwner != IntPtr.Zero)
                {
                    LocalFree(sidOwner);
                }
            }
        }

        public static string TryGetSidFromAppContainerV3(string appContainerName)
        {
            IntPtr sidPtr;
            int result = DeriveAppContainerSidFromAppContainerName(appContainerName, out sidPtr);

            if (result != 0) // Check if the function call was successful
            {
                Console.WriteLine($"Failed to get SID. Error: {result}");
                return "";
            }

            try
            {
                return new SecurityIdentifier(sidPtr).Value;
            }
            finally
            {
                // Free the SID using FreeSid function
                Marshal.FreeHGlobal(sidPtr);
            }
        }
    }
}
