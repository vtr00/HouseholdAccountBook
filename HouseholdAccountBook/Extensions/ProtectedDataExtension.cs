using System;
using System.Security.Cryptography;
using System.Text;

namespace HouseholdAccountBook.Extensions
{
    public class ProtectedDataExtension
    {
        /// <summary>
        /// 暗号化されたパスワードを生成する
        /// </summary>
        /// <param name="password">パスワード平文</param>
        /// <returns></returns>
        public static string EncryptPassword(string password)
        {
            // 暗号化（CurrentUserスコープ）
            byte[] encryptedBytes = [];
            try {
                encryptedBytes = ProtectedData.Protect(
                    Encoding.UTF8.GetBytes(password),
                    null,
                    DataProtectionScope.CurrentUser);
            }
            catch (CryptographicException) { }

            // バイト配列からBase64に変換
            return Convert.ToBase64String(encryptedBytes);
        }

        /// <summary>
        /// 復号化されたパスワードを取得する
        /// </summary>
        /// <param name="encryptedPassword">暗号化されたパスワード</param>
        /// <returns></returns>
        public static string DecryptPassword(string encryptedPassword)
        {
            // Base64からバイト配列に変換
            byte[] encryptedBytes = Convert.FromBase64String(encryptedPassword);

            // 復号化（CurrentUserスコープ）
            byte[] decryptedBytes = [];
            try {
                decryptedBytes = ProtectedData.Unprotect(
                    encryptedBytes,
                    null,
                    DataProtectionScope.CurrentUser);
            }
            catch (CryptographicException) { }

            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}
