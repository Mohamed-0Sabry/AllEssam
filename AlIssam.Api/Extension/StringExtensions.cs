using System.Text.RegularExpressions;

namespace AlIssam.API.Extension
{
    public static class StringExtensions
    {
        public static string GenerateSlug(this string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            // Convert to lowercase
            var slug = name.ToLower();

            // Replace spaces with hyphens
            slug = slug.Replace(" ", "-");

            // Remove special characters (allow Arabic, English, digits, and hyphens)
            slug = Regex.Replace(slug, @"[^\p{IsArabic}a-z0-9\-]", "");

            return slug;
        }
    }
}
