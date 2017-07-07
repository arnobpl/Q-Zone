using System.Text.RegularExpressions;

namespace Q_Zone.Models.Utility
{
    public static class ValidFormat
    {
        public static bool IsEmailFormat(string emailString) {
            if (string.IsNullOrEmpty(emailString)) return false;
            return Regex.IsMatch(emailString,
                @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z",
                RegexOptions.IgnoreCase);
        }
    }
}