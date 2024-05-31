using System.Runtime.InteropServices;
using System.Text;
using System.Security.Principal;
namespace AppToLaunch
{
    internal class TokenInfo
    {
        public static void PrintTokenSidInfo()
        {
            var currentProcessPtr = Win32Api.GetCurrentProcessPtr();

            bool ret = false;
            ret = Win32Api.OpenProcessToken(currentProcessPtr, Win32Api.TOKEN_QUERY, out var processTokenPtr);

            ret = Win32Api.GetTokenInformation(
                processTokenPtr,
                Win32Api.TOKEN_INFORMATION_CLASS.TokenGroupsAndPrivileges,
                IntPtr.Zero,
                0,
                out var tokenInformationLength);

            var groupsAndPrivilegesInfoPtr = Marshal.AllocHGlobal(tokenInformationLength);

            ret = Win32Api.GetTokenInformation(
                processTokenPtr,
                Win32Api.TOKEN_INFORMATION_CLASS.TokenGroupsAndPrivileges,
                groupsAndPrivilegesInfoPtr,
                tokenInformationLength,
                out _);

            Win32Api.TOKEN_GROUPS_AND_PRIVILEGES groupsAndPrivilegesInfo = (Win32Api.TOKEN_GROUPS_AND_PRIVILEGES)Marshal.PtrToStructure(
                groupsAndPrivilegesInfoPtr,
                typeof(Win32Api.TOKEN_GROUPS_AND_PRIVILEGES))!;

            //for (int i = 0; i < groupsAndPrivilegesInfo.PrivilegeCount; i++)
            //{
            //    Win32Api.LUID_AND_ATTRIBUTES Privilege = (Win32Api.LUID_AND_ATTRIBUTES)Marshal.PtrToStructure(
            //        groupsAndPrivilegesInfo.Privileges + i * Marshal.SizeOf(typeof(Win32Api.LUID_AND_ATTRIBUTES)),
            //        typeof(Win32Api.LUID_AND_ATTRIBUTES))!;

            //    StringBuilder name = new(50);
            //    uint cchName = 50;
            //    ret = Win32Api.LookupPrivilegeName(null, ref Privilege.Luid, name, ref cchName);
            //    Console.WriteLine(name);
            //}

            //for (int i = 0; i < groupsAndPrivilegesInfo.SidCount; i++)
            //{
            //    Win32Api.SID_AND_ATTRIBUTES sid = (Win32Api.SID_AND_ATTRIBUTES)Marshal.PtrToStructure(
            //        groupsAndPrivilegesInfo.Sids + i * Marshal.SizeOf(typeof(Win32Api.SID_AND_ATTRIBUTES)),
            //        typeof(Win32Api.SID_AND_ATTRIBUTES))!;

            //    string sidString = new StringBuilder(100).ToString();
            //    ret = Win32Api.ConvertSidToStringSid(sid.Sid, ref sidString);
            //    try
            //    {
            //        string account = new SecurityIdentifier(sidString).Translate(typeof(NTAccount)).ToString();

            //        if (account.Contains("BEIJERELC"))
            //        {
            //            continue;
            //        }

            //        Console.WriteLine($"{account}: {sidString}{Environment.NewLine}" +
            //            $"{sid.Attributes}");
            //    }
            //    catch (Exception)
            //    {
            //        Console.WriteLine($"Could not translate SID: {sidString} to an account");
            //    }
            //}

            Console.WriteLine($"{Environment.NewLine}----------Printing Restricted----------{Environment.NewLine}");
            for (int i = 0; i < groupsAndPrivilegesInfo.RestrictedSidCount; i++)
            {
                Win32Api.SID_AND_ATTRIBUTES restrictedSid = (Win32Api.SID_AND_ATTRIBUTES)Marshal.PtrToStructure(
                    groupsAndPrivilegesInfo.RestrictedSids + i * Marshal.SizeOf(typeof(Win32Api.SID_AND_ATTRIBUTES)),
                    typeof(Win32Api.SID_AND_ATTRIBUTES))!;

               

                string sidString = new StringBuilder(100).ToString();
                ret = Win32Api.ConvertSidToStringSid(restrictedSid.Sid, ref sidString);
                try
                {
                    string account = new SecurityIdentifier(sidString).Translate(typeof(NTAccount)).ToString();

                    if (account.Contains("BEIJERELC"))
                    {
                        continue;
                    }

                    Console.WriteLine(sidString);
                    Console.WriteLine(account);
                }
                catch (Exception)
                {
                    Console.WriteLine($"Could not translate SID: {sidString} to an account");
                }
            }


            Marshal.FreeHGlobal(groupsAndPrivilegesInfoPtr);
        }
    }
}
