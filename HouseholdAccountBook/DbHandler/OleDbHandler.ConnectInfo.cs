using HouseholdAccountBook.DbHandler.Abstract;

namespace HouseholdAccountBook.DbHandler
{
    public partial class OleDbHandler : DbHandlerBase
    {
        /// <summary>
        /// 接続情報
        /// </summary>
        public new class ConnectInfo : DbHandlerBase.ConnectInfo
        {
            /// <summary>
            /// プロバイダ
            /// </summary>
            public string Provider { get; set; }
            /// <summary>
            /// ファイルパス
            /// </summary>
            public string FilePath { get; set; }
        }
    }
}
