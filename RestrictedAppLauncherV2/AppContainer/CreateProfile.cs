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

        [DllImport("userenv.dll", SetLastError = true)]
        public static extern int CreateAppContainerProfile(
        string appContainerName,
        string displayName,
        string description,
        string packageFamilyName,
        IntPtr reserved);

        [DllImport("kernel32.dll")]
        private static extern void LocalFree(IntPtr hMem);
        #endregion

        public static string GetOrCreateAppContainerProfileSid(string appContainerName)
        {
            var containerSid = SidHelper.TryGetSidFromAppContainerV3(appContainerName);

            if (string.IsNullOrEmpty(containerSid))
            {
                // Define AppContainer parameters
                string displayName = "My App Container";
                string description = "A sample App Container for demonstration purposes.";
                string[] capabilities = [];// new string[] { "internetClient" }; // Add capabilities as needed

                // Create the AppContainer profile
                //if (CreateAppContainerProfile(appContainerName, displayName, description, IntPtr.Zero, (uint)capabilities.Length, capabilities, out IntPtr appContainerSid) == 0)
                if (CreateAppContainerProfile(appContainerName, displayName, description, null, IntPtr.Zero) == 0)
                {
                    Console.WriteLine("AppContainer created successfully.");

                    // Retrieve the SID as a string
                    //ConvertSidToStringSid(appContainerSid, out string newContainerSid);
                    // Free the allocated memory for the SID
                    //LocalFree(appContainerSid);


                    var newContainerSid = SidHelper.TryGetSidFromAppContainerV3(appContainerName);
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
    }
}
