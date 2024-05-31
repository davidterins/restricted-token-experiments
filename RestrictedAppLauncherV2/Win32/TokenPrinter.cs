using RestrictedAppLauncherV2.Win32.Native;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;

namespace RestrictedAppLauncherV2.Win32
{
    internal class TokenPrinter
    {
        public static IEnumerable<string> GetProcessTokenSidStrings(IntPtr processTokenPtr)
        {
            var processSidStrings = new List<string>();

            var ret = TokensApi.GetTokenInformation(
                processTokenPtr,
                TokensApi.TOKEN_INFORMATION_CLASS.TokenGroupsAndPrivileges,
                IntPtr.Zero,
                0,
                out var tokenInformationLength);

            var groupsAndPrivilegesInfoPtr = Marshal.AllocHGlobal(tokenInformationLength);

            ret = TokensApi.GetTokenInformation(
                processTokenPtr,
                TokensApi.TOKEN_INFORMATION_CLASS.TokenGroupsAndPrivileges,
                groupsAndPrivilegesInfoPtr,
                tokenInformationLength,
                out _);

            Win32Api.TOKEN_GROUPS_AND_PRIVILEGES groupsAndPrivilegesInfo = (Win32Api.TOKEN_GROUPS_AND_PRIVILEGES)Marshal.PtrToStructure(
                groupsAndPrivilegesInfoPtr,
                typeof(Win32Api.TOKEN_GROUPS_AND_PRIVILEGES))!;


            for (int i = 0; i < groupsAndPrivilegesInfo.SidCount; i++)
            {
                TokensApi.SID_AND_ATTRIBUTES sid = (TokensApi.SID_AND_ATTRIBUTES)Marshal.PtrToStructure(
                    groupsAndPrivilegesInfo.Sids + i * Marshal.SizeOf(typeof(TokensApi.SID_AND_ATTRIBUTES)),
                    typeof(TokensApi.SID_AND_ATTRIBUTES))!;

                string sidString = new StringBuilder(100).ToString();
                ret = TokensApi.ConvertSidToStringSid(sid.Sid, ref sidString);
                processSidStrings.Add(sidString);
            }

            Marshal.FreeHGlobal(groupsAndPrivilegesInfoPtr);

            return processSidStrings;
        }

        public static void PrintTokenSidInfo(IntPtr processTokenPtr)
        {
            bool ret = false;

            ret = TokensApi.GetTokenInformation(
                processTokenPtr,
                TokensApi.TOKEN_INFORMATION_CLASS.TokenGroupsAndPrivileges,
                IntPtr.Zero,
                0,
                out var tokenInformationLength);

            var groupsAndPrivilegesInfoPtr = Marshal.AllocHGlobal(tokenInformationLength);

            ret = TokensApi.GetTokenInformation(
                processTokenPtr,
                TokensApi.TOKEN_INFORMATION_CLASS.TokenGroupsAndPrivileges,
                groupsAndPrivilegesInfoPtr,
                tokenInformationLength,
                out _);

            Win32Api.TOKEN_GROUPS_AND_PRIVILEGES groupsAndPrivilegesInfo = (Win32Api.TOKEN_GROUPS_AND_PRIVILEGES)Marshal.PtrToStructure(
                groupsAndPrivilegesInfoPtr,
                typeof(Win32Api.TOKEN_GROUPS_AND_PRIVILEGES))!;

            for (int i = 0; i < groupsAndPrivilegesInfo.SidCount; i++)
            {
                TokensApi.SID_AND_ATTRIBUTES sid = (TokensApi.SID_AND_ATTRIBUTES)Marshal.PtrToStructure(
                    groupsAndPrivilegesInfo.Sids + i * Marshal.SizeOf(typeof(TokensApi.SID_AND_ATTRIBUTES)),
                    typeof(TokensApi.SID_AND_ATTRIBUTES))!;

                string sidString = new StringBuilder(100).ToString();
                ret = TokensApi.ConvertSidToStringSid(sid.Sid, ref sidString);
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

            Console.WriteLine($"{Environment.NewLine}----------Printing Restricted----------{Environment.NewLine}");
            for (int i = 0; i < groupsAndPrivilegesInfo.RestrictedSidCount; i++)
            {
                TokensApi.SID_AND_ATTRIBUTES restrictedSid = (TokensApi.SID_AND_ATTRIBUTES)Marshal.PtrToStructure(
                    groupsAndPrivilegesInfo.RestrictedSids + i * Marshal.SizeOf(typeof(TokensApi.SID_AND_ATTRIBUTES)),
                    typeof(TokensApi.SID_AND_ATTRIBUTES))!;



                string sidString = new StringBuilder(100).ToString();
                ret = TokensApi.ConvertSidToStringSid(restrictedSid.Sid, ref sidString);
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
