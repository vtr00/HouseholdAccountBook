using HouseholdAccountBook.Infrastructure.DB;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers.Abstract;
using HouseholdAccountBook.Infrastructure.Logger;
using Microsoft.Data.Sqlite;
using System;
using System.IO;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Models.DbHandlers
{
    /// <summary>
    /// SQLite DB Hander
    /// </summary>
    public partial class SQLiteDbHandler : DbHandlerBase
    {
        /// <summary>
        /// pragma設定
        /// </summary>
        public class PragmaConfiguration
        {
            /// <summary>
            /// キャッシュサイズの最大値[MB]
            /// </summary>
            public uint CacheSize { get; set; } = 64;

            /// <summary>
            /// メモリマップドI/Oサイズの最大値[MB]
            /// </summary>
            public uint MmapSize { get; set; } = 256;
        }

        #region フィールド
        /// <summary>
        /// 接続文字列
        /// </summary>
        private const string mStringFormat = @"Data Source={0};";
        #endregion

        #region プロパティ
        /// <summary>
        /// DBファイルパス
        /// </summary>
        public string DbFilePath => (this.mConnectInfoBase as ConnectInfo).FilePath;

        /// <summary>
        /// 最適化設定を行うか
        /// </summary>
        public bool Optimizing { get; init; } = true;
        /// <summary>
        /// gragma設定
        /// </summary>
        public static PragmaConfiguration Config { get; set; } = new();
        #endregion

        /// <summary>
        /// <see cref="SQLiteDbHandler"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="info">接続情報</param>
        /// <param name="optimizing">最適化設定を行うか</param>
        public SQLiteDbHandler(ConnectInfo info, bool optimizing) : this(info.FilePath, optimizing) => this.mConnectInfoBase = info;

        /// <summary>
        /// <see cref="SQLiteDbHandler"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <param name="optimizing">最適化設定を行うか</param>
        private SQLiteDbHandler(string filePath, bool optimizing) : base(new SqliteConnection(string.Format(mStringFormat, filePath)))
        {
            using FuncLog funcLog = new(new { filePath, optimizing }, Log.LogLevel.Trace);

            this.LibKind = DBLibraryKind.SQLite;
            this.Optimizing = optimizing;

            // SQLiteはファイルがない状態で接続するとファイルが作成される仕様のため、ファイルがない場合は接続しない
            if (!File.Exists(filePath)) {
                this.CanOpen = false;
            }
        }

        public override async Task<bool> OpenAsync(int timeoutMs = 0)
        {
            using FuncLog funcLog = new(new { timeoutMs }, Log.LogLevel.Trace);

            bool opened = await base.OpenAsync(timeoutMs);
            if (opened && this.Optimizing) {
                try {
                    // WALモード：Write-Ahead Logging による同時実行性の向上
                    _ = await this.ExecuteAsync("PRAGMA journal_mode = WAL;");

                    // キャッシュサイズ[KB]を設定
                    _ = await this.ExecuteAsync($"PRAGMA cache_size = -{Config.CacheSize * 1024};");

                    // メモリマッピング[bytes]を設定
                    _ = await this.ExecuteAsync($"PRAGMA mmap_size = {Config.MmapSize * 1024 * 1024};");

                    // 同期レベルを落とす（NORMAL: ディスクフラッシュを最小化）
                    _ = await this.ExecuteAsync("PRAGMA synchronous = NORMAL;");

                    // テンポラリストレージを使用
                    _ = await this.ExecuteAsync("PRAGMA temp_store = MEMORY;");
                }
                catch {
                    // 最適化設定が失敗してもDBは使用可能にする
                }
            }

            return opened;
        }

        public override async ValueTask DisposeAsync()
        {
            using FuncLog funcLog = new(new { }, Log.LogLevel.Trace);

            if (this.mConnection != null && this.Optimizing) {
                try {
                    // クエリ最適化レベルを上げる
                    _ = await this.ExecuteAsync("PRAGMA optimize;");
                }
                catch { }
            }
            await base.DisposeAsync();

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// スキーマバージョンを取得する
        /// </summary>
        /// <returns>スキーマバージョン</returns>
        public async Task<int> GetUserVersion()
        {
            using FuncLog funcLog = new(new { }, Log.LogLevel.Trace);

            return await this.QuerySingleAsync<int>($"PRAGMA USER_VERSION;");
        }

        /// <summary>
        /// スキーマバージョンを設定する
        /// </summary>
        /// <param name="version">スキーマバージョン</param>
        /// <returns></returns>
        public async Task<int> SetUserVersion(int version)
        {
            using FuncLog funcLog = new(new { version }, Log.LogLevel.Trace);

            return await this.ExecuteAsync($"PRAGMA USER_VERSION = {version};");
        }

        /// <summary>
        /// SQLiteテンプレートファイルをコピーして新規作成する
        /// </summary>
        /// <param name="sqliteFilePath">SQLiteファイルパス</param>
        /// <param name="sqliteBinary">SQLiteテンプレートファイルのバイナリデータ</param>
        /// <returns>ファイルが存在するか</returns>
        /// <remarks>ファイルが既に存在する場合は何もしない</remarks>
        public static bool CreateTemplateFile(string sqliteFilePath, byte[] sqliteBinary)
        {
            using FuncLog funcLog = new(new { sqliteFilePath }, Log.LogLevel.Trace);

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
