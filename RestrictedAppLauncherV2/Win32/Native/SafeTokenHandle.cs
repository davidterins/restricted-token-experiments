using Microsoft.Win32.SafeHandles;
using System;

namespace RestrictedAppLauncherV2.Win32.Native
{
    internal class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeTokenHandle(IntPtr handle)
            : base(true)
        {
            SetHandle(handle);
        }

        private SafeTokenHandle()
            : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            return ProcessApi.CloseHandle(handle);
        }
    }

    internal sealed class SafeThreadHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeThreadHandle(IntPtr handle)
            : base(true)
        {
            SetHandle(handle);
        }

        override protected bool ReleaseHandle()
        {
            return ProcessApi.CloseHandle(handle);
        }

    }
}
