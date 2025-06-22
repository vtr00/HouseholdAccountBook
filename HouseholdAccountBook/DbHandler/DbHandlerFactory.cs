using HouseholdAccountBook.DbHandler.Abstract;

namespace HouseholdAccountBook.DbHandler
{
    /// <summary>
    /// DAOファクトリ
    /// </summary>
    public class DbHandlerFactory
    {
        /// <summary>
        /// データベース種別
        /// </summary>
        private enum DatabaseType
        {
            /// <summary>
            /// SQLite
            /// </summary>
            SQLite,
            /// <summary>
            /// PostgreSQL
            /// </summary>
            PostgreSQL,
            /// <summary>
            /// OleDb
            /// </summary>
            OleDb,
            /// <summary>
            /// 未定義
            /// </summary>
            Undefined
        }

        /// <summary>
        /// 接続対象データベース
        /// </summary>
        private readonly DatabaseType target;
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
            if (info is DbHandlerNpgsql.ConnectInfo) {
                this.target = DatabaseType.PostgreSQL;
                this.info = info;
                return;
            }

            if (info is DbHandlerOleDb.ConnectInfo) {
                this.target = DatabaseType.OleDb;
                this.info = info;
                return;
            }

            if (info is DbHanderSQLite.ConnectInfo) {
                this.target = DatabaseType.SQLite;
                this.info = info;
                return;
            }

            this.target = DatabaseType.Undefined;
            this.info = null;
        }

        /// <summary>
        /// <see cref="DbHandlerBase"/> 生成
        /// </summary>
        /// <returns>DbHandler</returns>
        public DbHandlerBase Create()
        {
            try {
                DbHandlerBase daoBase;
                switch (this.target) {
                    case DatabaseType.SQLite:
                        daoBase = new DbHanderSQLite(this.info as DbHanderSQLite.ConnectInfo);
                        break;
                    case DatabaseType.PostgreSQL:
                        daoBase = new DbHandlerNpgsql(this.info as DbHandlerNpgsql.ConnectInfo);
                        break;
                    case DatabaseType.OleDb:
                        daoBase = new DbHandlerOleDb(this.info as DbHandlerOleDb.ConnectInfo);
                        break;
                    default:
                        daoBase = null;
                        break;
                }
                return daoBase;
            }
            catch (System.TimeoutException e) {
                throw e;
            }
        }
    }
}
