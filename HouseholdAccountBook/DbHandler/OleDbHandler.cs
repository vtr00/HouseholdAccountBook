using HouseholdAccountBook.DbHandler.Abstract;
using System.Data.OleDb;
using static HouseholdAccountBook.Others.DbConstants;

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
        private const string stringFormat = @"Provider={0};Data Source={1}";

        /// <summary>
        /// <see cref="OleDbHandler"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="info">接続情報</param>
        public OleDbHandler(ConnectInfo info) : this(info.Provider, info.FilePath) { }

        /// <summary>
        /// <see cref="OleDbHandler"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        public OleDbHandler(string provider, string filePath) : base(new OleDbConnection(string.Format(stringFormat, provider, filePath)))
        {
            this.LibKind = DBLibraryKind.OleDb;
        }
    }
}
