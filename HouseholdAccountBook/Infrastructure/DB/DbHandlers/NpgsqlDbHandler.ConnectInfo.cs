using HouseholdAccountBook.Infrastructure.DB;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers.Abstract;
using HouseholdAccountBook.Infrastructure.Utilities;
using HouseholdAccountBook.Infrastructure.Utilities.Attributes;
using HouseholdAccountBook.Infrastructure.Utilities.Extensions;

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
            public string UserName { get; set; }
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
            public string DatabaseName { get; set; }
            /// <summary>
            /// ロール名
            /// </summary>
            public string Role { get; set; }

            /// <summary>
            /// 暗号化済パスワード
            /// </summary>
            [IgnoreToString]
            public string EncryptedPassword { get; set; }

            /// <summary>
            /// パラメータが妥当か検証する
            /// </summary>
            /// <returns></returns>
            public bool CheckValidation()
            {
                bool ret = true;
                ret = ret && (this.Host?.IsValidDBIdentifier() ?? false);
                ret = ret && (this.Port is >= 0 and <= 65535);
                ret = ret && (this.UserName?.IsValidDBIdentifier() ?? false);
                ret = ret && (this.DatabaseName?.IsValidDBIdentifier() ?? false);
                ret = ret && (this.Role?.IsValidDBIdentifier() ?? false);

                return ret;
            }
        }
    }
}
