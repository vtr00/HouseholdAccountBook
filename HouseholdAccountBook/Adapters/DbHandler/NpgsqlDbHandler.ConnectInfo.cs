using HouseholdAccountBook.Adapters.DbHandler.Abstract;
using HouseholdAccountBook.Extensions;

namespace HouseholdAccountBook.DbHandler
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
            public string UserName { get; set; }
            /// <summary>
            /// パスワード
            /// </summary>
            public string Password
            {
                get => ProtectedDataExtension.DecryptPassword(this.EncryptedPassword);
                set => this.EncryptedPassword = ProtectedDataExtension.EncryptPassword(value);
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
            public string EncryptedPassword { get; set; }
        }
    }
}
