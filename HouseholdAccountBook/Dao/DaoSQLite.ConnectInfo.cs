namespace HouseholdAccountBook.Dao
{
    public partial class DaoSQLite : DaoBase
    {
        /// <summary>
        /// 接続情報
        /// </summary>
        public new class ConnectInfo : DaoBase.ConnectInfo
        {
            /// <summary>
            /// ファイルパス
            /// </summary>
            public string FilePath { get; set; }
        }
    }
}
