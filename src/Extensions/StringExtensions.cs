namespace FittsLaw.Extensions;

internal static class StringExtensions
{
#if VS_VERSION_18_0_OR_GREATER
    extension(string s)
    {
        public string ToPath(string replacement = "-")
        {
            var invalidChars = System.IO.Path.GetInvalidFileNameChars();
            string[] temp = s.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(replacement, temp);
        }
    }
#else
    public static string ToPath(this string s, string replacement = "-")
    {
        var invalidChars = System.IO.Path.GetInvalidFileNameChars();
        string[] temp = s.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries);
        return string.Join(replacement, temp);
    }
#endif
}
