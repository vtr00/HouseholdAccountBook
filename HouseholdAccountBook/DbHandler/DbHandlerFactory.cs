using HouseholdAccountBook.DbHandler.Abstract;
using static HouseholdAccountBook.ConstValue.ConstValue;

namespace HouseholdAccountBook.DbHandler
{
    /// <summary>
    /// DBハンドラファクトリ
    /// </summary>
    public class DbHandlerFactory
    {
        /// <summary>
        /// 接続対象データベース
        /// </summary>
        public DatabaseType Type { get; private set; } = DatabaseType.Undefined;
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
                this.Type = DatabaseType.PostgreSQL;
                this.info = info;
                return;
            }

            if (info is OleDbHandler.ConnectInfo) {
                this.Type = DatabaseType.OleDb;
                this.info = info;
                return;
            }

            if (info is SQLiteDbHandler.ConnectInfo) {
                this.Type = DatabaseType.SQLite;
                this.info = info;
                return;
            }

            this.Type = DatabaseType.Undefined;
            this.info = null;
        }

        /// <summary>
        /// <see cref="DbHandlerBase"/> 生成
        /// </summary>
        /// <returns>DbHandler</returns>
        public DbHandlerBase Create()
        {
            try {
                DbHandlerBase dbHandler;
                switch (this.Type) {
                    case DatabaseType.SQLite:
                        dbHandler = new SQLiteDbHandler(this.info as SQLiteDbHandler.ConnectInfo);
                        break;
                    case DatabaseType.PostgreSQL:
                        dbHandler = new NpgsqlDbHandler(this.info as NpgsqlDbHandler.ConnectInfo);
                        break;
                    case DatabaseType.OleDb:
                        dbHandler = new OleDbHandler(this.info as OleDbHandler.ConnectInfo);
                        break;
                    default:
                        dbHandler = null;
                        break;
                }
                return dbHandler;
            }
            catch (System.TimeoutException e) {
                throw e;
            }
        }
    }
}
