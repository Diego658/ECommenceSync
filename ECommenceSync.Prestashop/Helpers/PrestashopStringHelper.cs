using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECommenceSync.Prestashop.Helpers
{
    public static class PrestashopStringHelper
    {

        public static Dictionary<char, char> NameCharsRewrite = new Dictionary<char, char>("<>;=#{}".Select(c => new KeyValuePair<char, char>(c, '\0')));

        public static Dictionary<char, char> CharsRewriteLink = new Dictionary<char, char>()
        {
            {'µ', char.MinValue  },
            { ' ', '-' },
            {'Ñ', 'N' }  ,
            {'ñ', 'n' },
            {'á', 'a' },
            {'é', 'e' },
            {'í', 'i' },
            {'ó', 'o' },
            {'ú', 'u' },
            {'/', '-' },
            {'(', char.MinValue },
            {')', char.MinValue },
            {'`', char.MinValue },
            {char.Parse("'"), char.MinValue },
            {'+', char.MinValue },
            {'*', char.MinValue },
            { '"', char.MinValue} ,
            {'&', char.MinValue },
            {'#', char.MinValue },
            {'%', char.MinValue },
            {'$', char.MinValue },
            {'@', char.MinValue },
            {'!', char.MinValue },
            {char.Parse(@"\"), char.MinValue }
        };


        public static string GetCleanStringForName(this string text)
        {
            return GetCleanString(text, NameCharsRewrite);
        }

        public static string GetCleanString(this string text, Dictionary<char, char> charsMapping)
        {
            var builder = new StringBuilder(text.Length);
            foreach (var caracter in text)
            {
                if (charsMapping.ContainsKey(caracter))
                {

                    if (charsMapping[caracter] != char.MinValue)
                    {
                        builder.Append(charsMapping[caracter]);
                    }
                }
                else
                {
                    builder.Append(caracter);
                }

            }
            return builder.ToString();
        }

        public static string GetStringForLinkRewrite(this string text)
        {
            var builder = new StringBuilder(text.Length);
            foreach (var caracter in text.ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(caracter))
                {
                    if (CharsRewriteLink.ContainsKey(caracter))
                    {
                        if (CharsRewriteLink[caracter] != char.MinValue)
                        {
                            builder.Append(CharsRewriteLink[caracter]);
                        }

                    }
                    else
                    {
                        builder.Append(caracter);
                    }

                }
                else if (char.IsWhiteSpace(caracter))
                {
                    builder.Append("-");
                }
                else if (char.GetUnicodeCategory(caracter) == System.Globalization.UnicodeCategory.OpenPunctuation || char.GetUnicodeCategory(caracter) == System.Globalization.UnicodeCategory.ClosePunctuation)
                {
                    builder.Append("-");
                }


            }
            return builder.ToString();
        }


        public static string GetTruncated(this string text, int maxLength)
        {
            return text.Length > maxLength ? text.Substring(0, maxLength) : text;
        }

    }
}
