using System.Text.RegularExpressions;

namespace BOCS.Services
{
    public class YoutubeHelper
    {// বিভিন্ন URL ফরম্যাট থেকে শুধুই ভিডিও ID বের করবে
        public static string? ExtractId(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            // যদি সরাসরি 11-অক্ষরের আইডি হয়
            var trimmed = input.Trim();
            if (Regex.IsMatch(trimmed, @"^[A-Za-z0-9_\-]{11}$"))
                return trimmed;

            // youtu.be/ID
            var m = Regex.Match(trimmed, @"youtu\.be/([A-Za-z0-9_\-]{11})");
            if (m.Success) return m.Groups[1].Value;

            // youtube.com/watch?v=ID
            m = Regex.Match(trimmed, @"v=([A-Za-z0-9_\-]{11})");
            if (m.Success) return m.Groups[1].Value;

            // youtube.com/embed/ID
            m = Regex.Match(trimmed, @"embed/([A-Za-z0-9_\-]{11})");
            if (m.Success) return m.Groups[1].Value;

            return null;
        }
    }
}
