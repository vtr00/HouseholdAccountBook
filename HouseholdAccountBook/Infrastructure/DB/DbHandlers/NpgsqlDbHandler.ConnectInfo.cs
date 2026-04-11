using HouseholdAccountBook.Infrastructure.DB;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers.Abstract;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.Utilities;
using HouseholdAccountBook.Infrastructure.Utilities.Attributes;
using HouseholdAccountBook.Infrastructure.Utilities.Extensions;
using System;

namespace HouseholdAccountBook.Models.DbHandlers
{
    /// <summary>
    /// Npgsql DB Handler
    /// </summary>
    public partial class NpgsqlDbHandler : DbHandlerBase
    {
        /// <summary>
        /// 接続情報
        /// </summary>
        public class ConnectInfo : ConnectInfoBase
        {
            public ConnectInfo() => this.Kind = DBKind.PostgreSQL;

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
            /// <remarks><see cref="EncryptedPassword"/> のラッパープロパティ</remarks>
            [IgnoreToString]
            public string Password {
                get => ProtectedDataUtil.DecryptPassword(this.EncryptedPassword);
                set => this.EncryptedPassword = ProtectedDataUtil.EncryptPassword(value);
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
