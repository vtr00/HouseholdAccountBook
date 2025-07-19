using HouseholdAccountBook.DbHandler.Abstract;
using System.Data.OleDb;
using static HouseholdAccountBook.ConstValue.ConstValue;

namespace HouseholdAccountBook.DbHandler
{
    /// <summary>
    /// Ole Db Handler
    /// </summary>
    public partial class OleDbHandler : DbHandlerBase
    {
        /// <summary>
        /// 接続文字列
        /// </summary>
        private const string stringFormat = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0}";

        /// <summary>
        /// <see cref="OleDbHandler"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="info">接続情報</param>
        public OleDbHandler(ConnectInfo info) : this(info.FilePath) {
            this.Type = DatabaseType.OleDb;
        }

        /// <summary>
        /// <see cref="OleDbHandler"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        public OleDbHandler(string filePath) : base(new OleDbConnection(string.Format(stringFormat, filePath))) { }
    }
}
