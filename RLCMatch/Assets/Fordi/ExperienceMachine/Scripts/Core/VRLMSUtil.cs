

namespace RLC
{
    public static class VRLMSUtil
    {
        public static string FirstCharToUpper(this string s)
        {
            // Check for empty string.  
            if (string.IsNullOrEmpty(s))
                return string.Empty;
            else if (s.Length == 1)
                return char.ToUpper(s[0]) + "";
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        public static string Style(this string str, string style)
        {
            return "<style=" + style + ">" + str + "</style>";
        }
    }
}