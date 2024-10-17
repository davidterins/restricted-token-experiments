using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Security.Principal;

namespace RestrictedAppLauncherV2.AppContainer
{
    internal class CreateProfile
    {
        #region DLL imports
        // Import the necessary function from the Windows API
        //[DllImport("userenv.dll", SetLastError = true)]
        //private static extern uint CreateAppContainerProfile(
        //    string AppContainerName,
        //    string DisplayName,
        //    string Description,
        //    IntPtr Reserved,
        //    uint CapabilitiesCount,
        //    string[] Capabilities,
        //    out IntPtr AppContainerSid);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern int GetNamedSecurityInfo(
        string pObjectName,
        int ObjectType,
        int SecurityInfo,
        out IntPtr pSidOwner,
        IntPtr pSidGroup,
        IntPtr pDacl,
        IntPtr pSacl,
        out IntPtr ppSecurityDescriptor
    );

        [DllImport("userenv.dll", SetLastError = true)]
        public static extern int CreateAppContainerProfile(
        string appContainerName,
        string displayName,
        string description,
        string packageFamilyName,
        IntPtr reserved
    );

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool ConvertSidToStringSid(
            IntPtr Sid,
            out string StringSid);

        [DllImport("kernel32.dll")]
        private static extern void LocalFree(IntPtr hMem);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool LookupAccountName(
        string lpSystemName,
        string lpAccountName,
        IntPtr Sid,
        ref int cbSid,
        IntPtr ReferencedDomainName,
        ref int cchReferencedDomainName,
        out int peUse);
        #endregion

        public static string GetOrCreateAppContainerProfileSid(string appContainerName)
        {
            var containerSid = TryGetSidFromAppContainer(appContainerName);

            if (string.IsNullOrEmpty(containerSid))
            {
                // Define AppContainer parameters
                string displayName = "My App Container";
                string description = "A sample App Container for demonstration purposes.";
                string[] capabilities = new string[] { "internetClient" }; // Add capabilities as needed

                // Create the AppContainer profile
                //if (CreateAppContainerProfile(appContainerName, displayName, description, IntPtr.Zero, (uint)capabilities.Length, capabilities, out IntPtr appContainerSid) == 0)
                if (CreateAppContainerProfile(appContainerName, displayName, description, null, IntPtr.Zero) == 0)
                {
                    Console.WriteLine("AppContainer created successfully.");

                    // Retrieve the SID as a string
                    //ConvertSidToStringSid(appContainerSid, out string newContainerSid);
                    // Free the allocated memory for the SID
                    //LocalFree(appContainerSid);


                    var newContainerSid = TryGetSidFromAppContainer(appContainerName);
                    Console.WriteLine($"AppContainer SID: {newContainerSid}");
                    return newContainerSid;
                }
                else
                {
                    Console.WriteLine($"Failed to create AppContainer. Error: {Marshal.GetLastWin32Error()}");
                    return "";
                }
            }

            return containerSid;
        }

        static string TryGetExistingSid(string appContainerName)
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



        private static string TryGetSidFromAppContainer(string appContainerName)
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
    }
}
