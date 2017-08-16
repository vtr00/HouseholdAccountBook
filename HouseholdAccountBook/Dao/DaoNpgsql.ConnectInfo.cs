namespace HouseholdAccountBook.Dao
{
    public partial class DaoNpgsql : DaoBase
    {
        /// <summary>
        /// 接続情報
        /// </summary>
        public new class ConnectInfo : DaoBase.ConnectInfo
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
            public string Password { get; set; }
            /// <summary>
            /// データベース名
            /// </summary>
            public string DatabaseName { get; set; }
            /// <summary>
            /// ロール名
            /// </summary>
            public string Role { get; set; }
        }
    }
}
