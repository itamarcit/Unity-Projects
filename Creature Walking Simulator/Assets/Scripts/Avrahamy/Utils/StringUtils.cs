using System;

namespace Avrahamy.Utils {
    public static class StringUtils {
        public static bool IsLessThan(this string strA, string strB) {
            return string.CompareOrdinal(strA, strB) < 0;
        }

        public static bool IsLessThanOrEqual(this string strA, string strB) {
            return string.CompareOrdinal(strA, strB) <= 0;
        }

        public static bool IsGreaterThan(this string strA, string strB) {
            return string.CompareOrdinal(strA, strB) > 0;
        }

        public static bool IsGreaterThanOrEqual(this string strA, string strB) {
            return string.CompareOrdinal(strA, strB) >= 0;
        }

        /// <summary>
        /// Returns the substring between a prefix and a suffix, or null if not found.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="prefix"></param>
        /// <param name="suffix"></param>
        /// <param name="startOffset">An offset from the end of the prefix</param>
        /// <param name="endOffset">An offset from the start of the suffix</param>
        /// <returns></returns>
        public static string Substring(this string str, string prefix, string suffix, int startOffset = 0, int endOffset = 0) {
            var startIndex = str.IndexOf(prefix, StringComparison.Ordinal);
            if (startIndex < 0) return null;
            startIndex += startOffset + prefix.Length;
            var endIndex = str.IndexOf(suffix, startIndex, StringComparison.Ordinal);
            if (endIndex < 0) return null;
            endIndex += endOffset;
            return str.Substring(startIndex, endIndex - startIndex);
        }

        /// <summary>
        /// Returns the substring from the start of the string to a suffix, or null if not found.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="suffix"></param>
        /// <param name="endOffset">An offset from the start of the suffix</param>
        /// <returns></returns>
        public static string StartString(this string str, string suffix, int endOffset = 0) {
            var index = str.IndexOf(suffix, StringComparison.Ordinal);
            if (index < 0) return null;
            return str.Substring(0, index + endOffset);
        }

        /// <summary>
        /// Returns the substring between a prefix and the end of the string, or null if not found.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="prefix"></param>
        /// <param name="startOffset">An offset from the end of the prefix</param>
        /// <returns></returns>
        public static string EndString(this string str, string prefix, int startOffset = 0) {
            var index = str.IndexOf(prefix, StringComparison.Ordinal);
            if (index < 0) return null;
            return str.Substring(index + startOffset + prefix.Length);
        }
    }
}
