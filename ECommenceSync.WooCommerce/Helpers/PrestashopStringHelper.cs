using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommenceSync.WooCommerce.Helpers
{
    public static class WooStringHelper
    {
        public static Dictionary<char, char> CharsRewriteLink = new()
        {
            { 'µ', char.MinValue },
            { ' ', '-' },
            { 'Ñ', 'N' },
            { 'ñ', 'n' },
            { 'á', 'a' },
            { 'é', 'e' },
            { 'í', 'i' },
            { 'ó', 'o' },
            { 'ú', 'u' },
            { '/', '-' },
            { '(', char.MinValue },
            { ')', char.MinValue },
            { '`', char.MinValue },
            { char.Parse("'"), char.MinValue },
            { '+', char.MinValue },
            { '*', char.MinValue },
            { '"', char.MinValue },
            { '&', char.MinValue },
            { '#', char.MinValue },
            { '%', char.MinValue },
            { '$', char.MinValue },
            { '@', char.MinValue },
            { '!', char.MinValue },
            { char.Parse(@"\"), char.MinValue }
        };
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
    }
}
