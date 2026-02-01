using HouseholdAccountBook.Adapters.DbHandlers.Abstract;
using HouseholdAccountBook.Extensions;
using System;

namespace HouseholdAccountBook.Adapters.DbHandlers
{
    public partial class NpgsqlDbHandler : DbHandlerBase
    {
        /// <summary>
        /// 接続情報
        /// </summary>
        public new class ConnectInfo : DbHandlerBase.ConnectInfo
        {
            /// <summary>
            /// URI
            /// </summary>
            public string Host { get; set; }
            /// <summary>
            /// ポート番号
            /// </summary>
            public int Port { get; set; }
            /// <summary>
            /// ユーザー名
            /// </summary>
            public string UserName {
                get;
                set {
                    if (!SetValidIdentifier(ref field, value)) {
                        throw new ArgumentException("Invalid UserName", value);
                    }
                }
            }
            /// <summary>
            /// パスワード
            /// </summary>
            public string Password {
                get => ProtectedDataExtension.DecryptPassword(this.EncryptedPassword);
                set => this.EncryptedPassword = ProtectedDataExtension.EncryptPassword(value);
            }
            /// <summary>
            /// データベース名
            /// </summary>
            public string DatabaseName {
                get;
                set {
                    if (!SetValidIdentifier(ref field, value)) {
                        throw new ArgumentException("Invalid DatabaseName", value);
                    }
                }
            }
            /// <summary>
            /// ロール名
            /// </summary>
            public string Role {
                get;
                set {
                    if (!SetValidIdentifier(ref field, value)) {
                        throw new ArgumentException("Invalid Role", value);
                    }
                }
            }

            /// <summary>
            /// 暗号化済パスワード
            /// </summary>
            public string EncryptedPassword { get; set; }

            /// <summary>
            /// 適切な識別子かどうかを検証して、フィールドに設定する
            /// </summary>
            /// <param name="field">設定対象のフィールド</param>
            /// <param name="s">設定値</param>
            /// <returns>適切か</returns>
            public static bool SetValidIdentifier(ref string field, string s)
            {
                if (string.IsNullOrEmpty(s)) { return false; }
                if (s.Length > 63) { return false; }
                if (!char.IsLetter(s[0])) { return false; }

                foreach (var c in s) {
                    if (!(char.IsLetterOrDigit(c) || c == '_')) {
                        return false;
                    }
                }
                field = s;
                return true;
            }
        }
    }
}
