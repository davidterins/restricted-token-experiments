using System.Security.Principal;

namespace RestrictedAppLauncherV2.Win32
{
    internal static class Helpers
    {
        public static string UnQuote(this string v)
        {
            if (string.IsNullOrEmpty(v))
                return v;
            if (v[0] == '"' && v[v.Length - 1] == '"')
                return v.Substring(1, v.Length - 2);
            if (v[0] == '"' && v.Trim().EndsWith("\"", StringComparison.Ordinal))
                return v.Trim().UnQuote();
            if (v[0] == '"')
                return v.Substring(1);
            else
                return v;
        }

        internal static string ValidateUserName(string userName)
        {
            try
            {
                return new NTAccount(userName).Translate(typeof(SecurityIdentifier)).Translate(typeof(NTAccount)).Value;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Value \"{userName}\" is not a valid Username.", ex);
            }
        }

        internal static string GetSidFromUserName(string userName)
        {
            try
            {
                return new NTAccount(userName).Translate(typeof(SecurityIdentifier)).Value;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Value \"{userName}\" is not a valid Username.", ex);
            }
        }

    }
}
