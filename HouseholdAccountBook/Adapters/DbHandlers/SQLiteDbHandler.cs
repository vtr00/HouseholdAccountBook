using HouseholdAccountBook.Adapters.DbHandlers.Abstract;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Adapters.DbHandlers
{
    /// <summary>
    /// SQLite DB Hander
    /// </summary>
    public partial class SQLiteDbHandler : DbHandlerBase
    {
        /// <summary>
        /// 接続文字列
        /// </summary>
        private const string mStringFormat = @"Data Source={0}";

        /// <summary>
        /// <see cref="SQLiteDbHandler"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="info">接続情報</param>
        public SQLiteDbHandler(ConnectInfo info) : this(info.FilePath) { }

        /// <summary>
        /// <see cref="SQLiteDbHandler"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        public SQLiteDbHandler(string filePath) : base(new SqliteConnection(string.Format(mStringFormat, filePath)))
        {
            this.DBLibKind = DBLibraryKind.SQLite;
            this.DBKind = DBKind.SQLite;

            // SQLiteはファイルがない状態で接続するとファイルが作成される仕様のため、ファイルがない場合は接続しない
            if (!File.Exists(filePath)) {
                this.CanOpen = false;
            }
        }

        /// <summary>
        /// スキーマバージョンを取得する
        /// </summary>
        /// <returns>スキーマバージョン</returns>
        public async Task<int> GetUserVersion() => await this.QuerySingleAsync<int>($"PRAGMA USER_VERSION;");

        /// <summary>
        /// スキーマバージョンを設定する
        /// </summary>
        /// <param name="version">スキーマバージョン</param>
        /// <returns></returns>
        public async Task<int> SetUserVersion(int version) => await this.ExecuteAsync($"PRAGMA USER_VERSION = @UserVersion;", new { UserVersion = version });

        /// <summary>
        /// SQLiteテンプレートファイルをコピーして新規作成する
        /// </summary>
        /// <param name="sqliteFilePath">SQLiteファイルパス</param>
        /// <param name="sqliteBinary">SQLiteテンプレートファイルのバイナリデータ</param>
        /// <returns>ファイルが存在するか</returns>
        /// <remarks>ファイルが既に存在する場合は何もしない</remarks>
        public static bool CreateTemplateFile(string sqliteFilePath, byte[] sqliteBinary)
        {
            bool exists = File.Exists(sqliteFilePath);
            if (!exists) {
                try {
                    File.WriteAllBytes(sqliteFilePath, sqliteBinary);
                    exists = true;
                }
                catch { }
            }

            return exists;
        }
    }
}
