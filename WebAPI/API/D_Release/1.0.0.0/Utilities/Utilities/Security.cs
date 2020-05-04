using System;
using System.Text;
using System.Text.RegularExpressions;

namespace PulseCheck.Utilities
{
    public static class Security
    {
        public static string GeneratePassword()
        {
            var minLength = Convert.ToInt32(Settings.GetSetting(Settings.Constants.PASSWORD_MINIMUM_LENGTH));
            var password = new StringBuilder(minLength);
            while (password.Length < minLength)
            {
                var tmpPassword = System.Web.Security.Membership.GeneratePassword(
                    minLength,
                    Convert.ToInt32(Settings.GetSetting(Settings.Constants.PASSWORD_MINIMUM_SPECIAL_CHARS))
                );
                tmpPassword = Regex.Replace(tmpPassword.ToUpper(), @"[^A-Z0-9]", m => "");
                foreach (var c in tmpPassword)
                {
                    password.Append(c);
                    if (password.Length == minLength)
                        break;
                }
            }

            return password.ToString();
        }
    }
}
