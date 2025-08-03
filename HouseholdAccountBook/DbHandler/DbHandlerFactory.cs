using HouseholdAccountBook.DbHandler.Abstract;
using static HouseholdAccountBook.Others.DbConstants;

namespace HouseholdAccountBook.DbHandler
{
    /// <summary>
    /// DBハンドラファクトリ
    /// </summary>
    public class DbHandlerFactory
    {
        /// <summary>
        /// ライブラリ種別
        /// </summary>
        public DBLibraryKind LibKind { get; private set; } = DBLibraryKind.Undefined;
        /// <summary>
        /// 接続情報
        /// </summary>
        private readonly DbHandlerBase.ConnectInfo info;

        /// <summary>
        /// <see cref="DbHandlerFactory"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="info">接続情報</param>
        public DbHandlerFactory(DbHandlerBase.ConnectInfo info)
        {
            if (info is NpgsqlDbHandler.ConnectInfo) {
                this.LibKind = DBLibraryKind.PostgreSQL;
                this.info = info;
                return;
            }

            if (info is SQLiteDbHandler.ConnectInfo) {
                this.LibKind = DBLibraryKind.SQLite;
                this.info = info;
                return;
            }

            if (info is OleDbHandler.ConnectInfo) {
                this.LibKind = DBLibraryKind.OleDb;
                this.info = info;
                return;
            }

            this.LibKind = DBLibraryKind.Undefined;
            this.info = null;
        }

        /// <summary>
        /// <see cref="DbHandlerBase"/> 生成
        /// </summary>
        /// <returns>DbHandler</returns>
        public DbHandlerBase Create()
        {
            try {
                DbHandlerBase dbHandler = this.LibKind switch {
                    DBLibraryKind.SQLite => new SQLiteDbHandler(this.info as SQLiteDbHandler.ConnectInfo),
                    DBLibraryKind.PostgreSQL => new NpgsqlDbHandler(this.info as NpgsqlDbHandler.ConnectInfo),
                    DBLibraryKind.OleDb => new OleDbHandler(this.info as OleDbHandler.ConnectInfo),
                    _ => null,
                };
                return dbHandler;
            }
            catch (System.TimeoutException) {
                throw;
            }
        }
    }
}
