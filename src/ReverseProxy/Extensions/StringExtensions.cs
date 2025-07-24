using System.Text.RegularExpressions;

namespace ReverseProxy
{
    /// <summary>Extensions for <see cref="string"/>.</summary>
    public static class StringExtensions
    {
        /// <summary>Replaces all occurrences of a specified pattern in a string with a replacement string.</summary>
        /// <param name="containingString">The string to search for the pattern in.</param>
        /// <param name="pattern">The pattern to search for.</param>
        /// <param name="replacementString">The replacement string.</param>
        /// <param name="replacementCount">The maximum number of replacements to make. Default is <see cref="int.MaxValue"/>.</param>
        /// <returns>The containingString with any replacements.</returns>
        public static string RegexReplace(this string containingString, string pattern, string replacementString, int replacementCount = int.MaxValue)
        {
            var regex = new Regex(Regex.Escape(pattern));
            var result = regex.Replace(containingString, replacementString, replacementCount);
            return result;
        }
    }
}
