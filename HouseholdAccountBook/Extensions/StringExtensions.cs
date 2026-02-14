namespace HouseholdAccountBook.Extensions
{
    /// <summary>
    /// <see cref="string"/> の拡張メソッドを提供します
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// 文字列がDBで有効な識別子であるかを検査する
        /// </summary>
        /// <param name="str">検査する文字列</param>
        /// <returns>有効な識別子であればtrue、そうでなければfalse</returns>
        public static bool IsValidDBIdentifier(this string str)
        {
            if (string.IsNullOrEmpty(str)) {
                return false;
            }
            if (str.Length > 63) {
                return false;
            }
            if (!(char.IsLetter(str[0]) || str[0] == '_')) {
                return false;
            }
            foreach (char s in str) {
                if (!(char.IsLetterOrDigit(s) || s == '_')) {
                    return false;
                }
            }
            return true;
        }
    }
}
