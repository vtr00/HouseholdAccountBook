using HouseholdAccountBook.DbHandler.Abstract;
using System.Data.SQLite;
using System.IO;
using static HouseholdAccountBook.Others.DbConstants;

namespace HouseholdAccountBook.DbHandler
{
    /// <summary>
    /// SQLite DB Hander
    /// </summary>
    public partial class SQLiteDbHandler : DbHandlerBase
    {
        /// <summary>
        /// 接続文字列
        /// </summary>
        private const string stringFormat = @"Data Source={0}";

        /// <summary>
        /// <see cref="SQLiteDbHandler"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="info">接続情報</param>
        public SQLiteDbHandler(ConnectInfo info) : this(info.FilePath) { }

        /// <summary>
        /// <see cref="SQLiteDbHandler"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        public SQLiteDbHandler(string filePath) : base(new SQLiteConnection(string.Format(stringFormat, filePath)))
        {
            this.DBLibKind = DBLibraryKind.SQLite;
            this.DBKind = DBKind.SQLite;

            // SQLiteはファイルがない状態で接続するとファイルが作成される仕様のため、ファイルがない場合は接続しない
            if (File.Exists(filePath)) {
                _ = this.Open();
            }
        }
    }
}
