using HouseholdAccountBook.Infrastructure.DB.DbHandlers.Abstract;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Models.DbHandlers;
using System;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Infrastructure.DB.DbHandlers
{
    /// <summary>
    /// DBハンドラファクトリ
    /// </summary>
    /// <remarks>接続情報に応じた <see cref="DbHandlerBase"/> の派生クラスを生成する</remarks>
    public class DbHandlerFactory
    {
        #region フィールド
        /// <summary>
        /// 接続情報
        /// </summary>
        private readonly DbHandlerBase.ConnectInfoBase mConnectInfo;
        #endregion

        #region プロパティ
        /// <summary>
        /// DBライブラリ種別
        /// </summary>
        public DBLibraryKind DBLibKind { get; protected set; }
        /// <summary>
        /// DB種別
        /// </summary>
        public DBKind DBKind => this.mConnectInfo.Kind;
        #endregion

        /// <summary>
        /// <see cref="DbHandlerFactory"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="info">接続情報</param>
        public DbHandlerFactory(DbHandlerBase.ConnectInfoBase info)
        {
            using FuncLog funcLog = new(new { info }, Log.LogLevel.Trace);

            if (info is NpgsqlDbHandler.ConnectInfo) {
                this.DBLibKind = DBLibraryKind.PostgreSQL;
                this.mConnectInfo = info;
                return;
            }

            if (info is SQLiteDbHandler.ConnectInfo) {
                this.DBLibKind = DBLibraryKind.SQLite;
                this.mConnectInfo = info;
                return;
            }

            if (info is OleDbHandler.ConnectInfo) {
                this.DBLibKind = DBLibraryKind.OleDb;
                this.mConnectInfo = info;
                return;
            }

            this.DBLibKind = DBLibraryKind.Undefined;
            this.mConnectInfo = null;
        }

        /// <summary>
        /// [非同期] <see cref="DbHandlerBase"/> を生成する
        /// </summary>
        /// <param name="timeoutMs">タイムアウト時間(ms)。0以下の場合は無制限</param>
        /// <returns>DbHandlers</returns>
        /// <exception cref="TimeoutException">接続タイムアウトが発生した場合</exception>
        /// <remarks>このタイミングではSQLiteファイルは生成されない</remarks>
        public async Task<DbHandlerBase> CreateAsync(int timeoutMs = 0)
        {
            using FuncLog funcLog = new(new { timeoutMs }, Log.LogLevel.Trace);

            try {
                DbHandlerBase dbHandler = this.DBLibKind switch {
                    DBLibraryKind.SQLite => new SQLiteDbHandler(this.mConnectInfo as SQLiteDbHandler.ConnectInfo),
                    DBLibraryKind.PostgreSQL => new NpgsqlDbHandler(this.mConnectInfo as NpgsqlDbHandler.ConnectInfo),
                    DBLibraryKind.OleDb => new OleDbHandler(this.mConnectInfo as OleDbHandler.ConnectInfo),
                    _ => null,
                };

                _ = await dbHandler.OpenAsync(timeoutMs);
                return dbHandler;
            }
            catch (TimeoutException) {
                throw;
            }
        }
    }
}
