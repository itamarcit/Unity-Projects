using UnityEngine;

namespace Avrahamy.Utils {
    public static class EmailUtils {
        public static void MailTo(string email, string subject, string body) {
            var url = $"mailto:{email}?subject={Escape(subject)}&body={Escape(body)}";
            DebugLog.LogError("Opening mailto URL: " + url);
            if (url.Length > 2040) {
                url = url.Substring(0, 2040);
            }
            Application.OpenURL(url);
        }

        // Uri.EscapeUriString is not strict enough for Gmail. Need to do that manually.
        public static string Escape(string uri) {
            return uri.Replace(" ", "%20")
                .Replace("\t", "%09")
                .Replace(".", "%2E")
                .Replace(":", "%3A")
                .Replace("/", "%2F")
                .Replace("(", "%28")
                .Replace(")", "%29")
                .Replace("+", "%2B")
                .Replace("-", "%2D")
                .Replace("\"", "%22")
                .Replace("\n", "%0A");
        }
    }
}
