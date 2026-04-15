using System.Text.RegularExpressions;
using Engzly.Application.Interfaces.Moderation;

namespace Engzly.Application.Common.Moderation
{
    public sealed class RegexContentPolicy : IContentPolicy
    {
        private static readonly Regex UrlPattern = new(
            @"(?ix)
              \b(
                  (?:https?://|ftp://|www\.)[^\s]+
                  |
                  [a-z0-9][a-z0-9\-]{1,63}\.(?:com|net|org|io|co|app|dev|ai|me|info|biz|xyz|online|site|store|eg|sa|ae|uk|us|de|fr|ru)(?:/[^\s]*)?
              )\b",
            RegexOptions.Compiled);

        private static readonly Regex PhonePattern = new(
            @"(?x)
              (?:\+?\d[\s\-\.\(\)]?){7,}
              |
              \b0?1[0125]\d{8}\b",
            RegexOptions.Compiled);

        private static readonly Regex EmailPattern = new(
            @"\b[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}\b",
            RegexOptions.Compiled);

        public ContentPolicyResult Check(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new ContentPolicyResult(true, null);

            if (UrlPattern.IsMatch(text))
                return new ContentPolicyResult(false, "External links are not allowed. All communication must stay inside Engzly.");

            if (EmailPattern.IsMatch(text))
                return new ContentPolicyResult(false, "Sharing email addresses is not allowed.");

            if (PhonePattern.IsMatch(text))
                return new ContentPolicyResult(false, "Sharing phone numbers is not allowed.");

            return new ContentPolicyResult(true, null);
        }
    }
}
