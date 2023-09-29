namespace ECommenceSync.Helper
{
    public static class StringHelper
    {
        public static string GetTruncated(this string text, int maxLength)
        {
            return text.Length > maxLength ? text.Substring(0, maxLength) : text;
        }
    }
}
