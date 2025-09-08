using HouseholdAccountBook.Models.DbHandler.Abstract;

namespace HouseholdAccountBook.DbHandler
{
    public partial class SQLiteDbHandler : DbHandlerBase
    {
        /// <summary>
        /// 接続情報
        /// </summary>
        public new class ConnectInfo : DbHandlerBase.ConnectInfo
        {
            /// <summary>
            /// ファイルパス
            /// </summary>
            public string FilePath { get; set; }
        }
    }
}
