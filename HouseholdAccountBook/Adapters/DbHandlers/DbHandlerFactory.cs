using HouseholdAccountBook.Adapters.DbHandlers.Abstract;
using HouseholdAccountBook.Enums;
using System;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Adapters.DbHandlers
{
    /// <summary>
    /// DBハンドラファクトリ
    /// </summary>
    /// <remarks>接続情報に応じた <see cref="DbHandlerBase"/> の派生クラスを生成する</remarks>
    public class DbHandlerFactory
    {
        /// <summary>
        /// DBライブラリ種別
        /// </summary>
        private readonly DBLibraryKind mDBLibKind;
        /// <summary>
        /// 接続情報
        /// </summary>
        private readonly DbHandlerBase.ConnectInfo mInfo;

        /// <summary>
        /// <see cref="DbHandlerFactory"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="info">接続情報</param>
        public DbHandlerFactory(DbHandlerBase.ConnectInfo info)
        {
            if (info is NpgsqlDbHandler.ConnectInfo) {
                this.mDBLibKind = DBLibraryKind.PostgreSQL;
                this.mInfo = info;
                return;
            }

            if (info is SQLiteDbHandler.ConnectInfo) {
                this.mDBLibKind = DBLibraryKind.SQLite;
                this.mInfo = info;
                return;
            }

            if (info is OleDbHandler.ConnectInfo) {
                this.mDBLibKind = DBLibraryKind.OleDb;
                this.mInfo = info;
                return;
            }

            this.mDBLibKind = DBLibraryKind.Undefined;
            this.mInfo = null;
        }

        /// <summary>
        /// [非同期] <see cref="DbHandlerBase"/> 生成
        /// </summary>
        /// <param name="timeoutMs">タイムアウト時間(ms)。0以下の場合は無制限</param>
        /// <returns>DbHandlers</returns>
        /// <exception cref="TimeoutException">接続タイムアウトが発生した場合</exception>
        /// <remarks>このタイミングではSQLiteファイルは生成されない</remarks>
        public async Task<DbHandlerBase> CreateAsync(int timeoutMs = 0)
        {
            try {
                DbHandlerBase dbHandler = this.mDBLibKind switch {
                    DBLibraryKind.SQLite => new SQLiteDbHandler(this.mInfo as SQLiteDbHandler.ConnectInfo),
                    DBLibraryKind.PostgreSQL => new NpgsqlDbHandler(this.mInfo as NpgsqlDbHandler.ConnectInfo),
                    DBLibraryKind.OleDb => new OleDbHandler(this.mInfo as OleDbHandler.ConnectInfo),
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
