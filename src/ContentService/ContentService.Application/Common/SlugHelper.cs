using System.Text;
using System.Text.RegularExpressions;

namespace ContentService.Application.Common
{
    public static partial class SlugHelper
    {
        public static string Slugify(string input)
        {
            input = input.Trim().ToLowerInvariant();

            // Türkçe/Latin benzeri karakter normalize
            input = input
                .Replace('ğ', 'g').Replace('ü', 'u').Replace('ş', 's')
                .Replace('ı', 'i').Replace('ö', 'o').Replace('ç', 'c');

            var sb = new StringBuilder();
            foreach (var ch in input)
            {
                if (char.IsLetterOrDigit(ch)) sb.Append(ch);
                else if (char.IsWhiteSpace(ch) || ch == '-' || ch == '_') sb.Append('-');
            }
            var slug = Regex.Replace(sb.ToString(), "-{2,}", "-").Trim('-');
            return string.IsNullOrWhiteSpace(slug) ? "content" : slug;
        }

        public static bool IsValidSlug(string slug) =>
            Regex.IsMatch(slug, "^[a-z0-9]+(?:-[a-z0-9]+)*$");
    }
}
