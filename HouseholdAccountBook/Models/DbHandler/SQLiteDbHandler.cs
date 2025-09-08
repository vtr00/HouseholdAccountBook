using HouseholdAccountBook.Models.DbHandler.Abstract;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Threading.Tasks;
using static HouseholdAccountBook.Models.DbConstants;

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
        public SQLiteDbHandler(string filePath) : base(new SqliteConnection(string.Format(stringFormat, filePath)))
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
        public async Task<int> GetUserVersion()
        {
            return await this.QuerySingleAsync<int>($"PRAGMA USER_VERSION;");
        }

        /// <summary>
        /// スキーマバージョンを設定する
        /// </summary>
        /// <param name="version">スキーマバージョン</param>
        /// <returns></returns>
        public async Task<int> SetUserVersion(int version)
        {
            return await this.ExecuteAsync($"PRAGMA USER_VERSION = @UserVersion;", new { UserVersion = version });
        }
    }
}
