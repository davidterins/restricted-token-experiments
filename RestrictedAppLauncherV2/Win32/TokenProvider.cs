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

        public TokenProvider RestrictToken()
        {
            var restrictedSids = TokenPrinter.GetProcessTokenSidStrings(Token.DangerousGetHandle()).ToList();

            var restrictedSidsAllocationSize = Marshal.SizeOf<SID_AND_ATTRIBUTES>() * restrictedSids.Count;
            IntPtr restrictedSidsPtr = Marshal.AllocHGlobal(restrictedSidsAllocationSize);

            for (int i = 0; i < restrictedSids.Count; i++)
            {
                var restrictedSidString = restrictedSids[i];
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
                    //(uint)diabledSidStrings.Count, disabledSidsPtr,
                    0, IntPtr.Zero,
                    0, IntPtr.Zero,
                    (uint)restrictedSids.Count, restrictedSidsPtr,
                    //0, IntPtr.Zero,
                    out SafeTokenHandle restrictedToken) && !false)
                throw new Win32Exception();

            Token.Close();
            Token = restrictedToken;

            TokenPrinter.PrintTokenSidInfo(Token.DangerousGetHandle());
            return this;
        }

        public void Dispose()
        {
        }
    }
}