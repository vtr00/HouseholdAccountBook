namespace HouseholdAccountBook.Dao
{
    public partial class DaoBuilder
    {
        /// <summary>
        /// 接続対象
        /// </summary>
        private enum Target
        {
            /// <summary>
            /// SQLite
            /// </summary>
            SQLite,
            /// <summary>
            /// PostgreSQL
            /// </summary>
            PostgreSQL,
            /// <summary>
            /// OleDb
            /// </summary>
            OleDb,
            /// <summary>
            /// 未定義
            /// </summary>
            Undefined
        }
    }
}
