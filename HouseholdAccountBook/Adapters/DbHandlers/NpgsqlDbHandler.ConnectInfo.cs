using HouseholdAccountBook.Adapters.DbHandlers.Abstract;
using HouseholdAccountBook.Adapters.Logger;
using HouseholdAccountBook.Attributes;
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
                        throw new ArgumentException($"Invalid UserName: {value}", nameof(this.UserName));
                    }
                }
            }
            /// <summary>
            /// パスワード
            /// </summary>
            [IgnoreToString]
            public string Password {
                get => ProtectedDataExtensions.DecryptPassword(this.EncryptedPassword);
                set => this.EncryptedPassword = ProtectedDataExtensions.EncryptPassword(value);
            }
            /// <summary>
            /// データベース名
            /// </summary>
            public string DatabaseName {
                get;
                set {
                    if (!SetValidIdentifier(ref field, value)) {
                        throw new ArgumentException($"Invalid DatabaseName: {value}", nameof(this.DatabaseName));
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
                        throw new ArgumentException($"Invalid Role: {value}", nameof(this.Role));
                    }
                }
            }

            /// <summary>
            /// 暗号化済パスワード
            /// </summary>
            [IgnoreToString]
            public string EncryptedPassword { get; set; }

            /// <summary>
            /// 適切な識別子かどうかを検証して、フィールドに設定する
            /// </summary>
            /// <param name="field">設定対象のフィールド</param>
            /// <param name="str">設定値</param>
            /// <returns>適切か</returns>
            public static bool SetValidIdentifier(ref string field, string str)
            {
                using FuncLog funcLog = new(new { str }, Log.LogLevel.Trace);

                if (!str.IsValidDBIdentifier()) {
                    return false;
                }

                field = str;
                return true;
            }
        }
    }
}
