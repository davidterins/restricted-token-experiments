using RestrictedAppLauncherV2.Win32.Native;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static RestrictedAppLauncherV2.Win32.Native.ProcessApi;
using static RestrictedAppLauncherV2.Win32.Native.TokensApi;

namespace RestrictedAppLauncherV2.Win32
{
    internal class TokenProvider : IDisposable
    {
        private SafeTokenHandle Token;
        public SafeTokenHandle GetToken() => Token;

        public static TokenProvider OpenCurrentProcessToken(uint access =
            TOKEN_DUPLICATE |
            TOKEN_ADJUST_DEFAULT |
            TOKEN_QUERY |
            TOKEN_ASSIGN_PRIMARY |
            TOKEN_ADJUST_PRIVILEGES |
            TOKEN_ADJUST_GROUPS
            )
        {
            if (!OpenProcessToken(Process.GetCurrentProcess().Handle,
                    access,
                    out IntPtr existingProcessToken))
                throw new Win32Exception();

            if (existingProcessToken == IntPtr.Zero)
                throw new Win32Exception();

            return new TokenProvider() { Token = new SafeTokenHandle(existingProcessToken) };
        }

        public TokenProvider RestrictTokenSids()
        {
            var processTokenSidStrings = TokenPrinter.GetProcessTokenSidStrings(Token.DangerousGetHandle()).ToList();

            var restrictedSidsAllocationSize = Marshal.SizeOf<SID_AND_ATTRIBUTES>() * processTokenSidStrings.Count;
            IntPtr restrictedSidsPtr = Marshal.AllocHGlobal(restrictedSidsAllocationSize);

            for (int i = 0; i < processTokenSidStrings.Count; i++)
            {
                var restrictedSidString = processTokenSidStrings[i];
                IntPtr newPtr = restrictedSidsPtr + i * Marshal.SizeOf<SID_AND_ATTRIBUTES>();

                ConvertStringSidToSid(restrictedSidString, out var restrictedSidPtr);

                SID_AND_ATTRIBUTES restrictedSidAndAttribute = new()
                {
                    Sid = restrictedSidPtr,
                    Attributes = 0
                };

                Marshal.StructureToPtr(restrictedSidAndAttribute, newPtr, true);
            }

            if (!CreateRestrictedToken(
                    Token,
                    0,
                    0, IntPtr.Zero,
                    0, IntPtr.Zero,
                    (uint)processTokenSidStrings.Count, restrictedSidsPtr,
                    //0, IntPtr.Zero, //< ------------Uncomment this and comment out the line above and it works...
                    out SafeTokenHandle restrictedToken) && !false)
                throw new Win32Exception();

            Token.Close();
            Token = restrictedToken;

            TokenPrinter.PrintTokenSidInfo(Token.DangerousGetHandle());
            return this;
        }

        //public TokenProvider RestrictForAppContainer(string appContainerSid)
        //{
        //    var processTokenSidStrings = TokenPrinter.GetProcessTokenSidStrings(Token.DangerousGetHandle()).ToList();

        //    var restrictedSidsAllocationSize = Marshal.SizeOf<SID_AND_ATTRIBUTES>() * processTokenSidStrings.Count;
        //    IntPtr restrictedSidsPtr = Marshal.AllocHGlobal(restrictedSidsAllocationSize);

        //    for (int i = 0; i < processTokenSidStrings.Count; i++)
        //    {
        //        var restrictedSidString = processTokenSidStrings[i];
        //        IntPtr newPtr = restrictedSidsPtr + i * Marshal.SizeOf<SID_AND_ATTRIBUTES>();

        //        ConvertStringSidToSid(restrictedSidString, out var restrictedSidPtr);

        //        SID_AND_ATTRIBUTES restrictedSidAndAttribute = new()
        //        {
        //            Sid = restrictedSidPtr,
        //            Attributes = 0
        //        };

        //        Marshal.StructureToPtr(restrictedSidAndAttribute, newPtr, true);
        //    }

        //    CreateRestrictedToken(Token, NativeMethods.TOKEN_RESTRICTED, 0, IntPtr.Zero, 0, IntPtr.Zero, 1, new SID_AND_ATTRIBUTES { Sid = appContainerSid, Attributes = 0 }, out restrictedToken)

        //    if (!CreateRestrictedToken(
        //            Token,
        //            0,
        //            0, IntPtr.Zero,
        //            0, IntPtr.Zero,
        //            (uint)processTokenSidStrings.Count, restrictedSidsPtr,
        //            //0, IntPtr.Zero, //< ------------Uncomment this and comment out the line above and it works...
        //            out SafeTokenHandle restrictedToken) && !false)
        //        throw new Win32Exception();

        //    Token.Close();
        //    Token = restrictedToken;

        //    TokenPrinter.PrintTokenSidInfo(Token.DangerousGetHandle());
        //    return this;
        //}

        public void Dispose()
        {
            Token.Dispose();
        }
    }
}