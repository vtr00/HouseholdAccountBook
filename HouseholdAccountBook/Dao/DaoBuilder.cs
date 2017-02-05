namespace HouseholdAccountBook.Dao
{
    /// <summary>
    /// DAOビルダ
    /// </summary>
    public class DaoBuilder
    {
        /// <summary>
        /// 接続対象
        /// </summary>
        private enum Target
        {
            SQLite,
            PostgreSQL,
            OleDb,
            Undefined
        }
        /// <summary>
        /// 接続対象
        /// </summary>
        private Target target;
        /// <summary>
        /// 接続情報
        /// </summary>
        private DaoBase.ConnectInfo info;

        /// <summary>
        /// DAOビルダ
        /// </summary>
        /// <param name="info">接続情報</param>
        public DaoBuilder(DaoBase.ConnectInfo info)
        {
            this.target = Target.Undefined;
            this.info = null;

            DaoNpgsql.ConnectInfo npgsqlInfo = info as DaoNpgsql.ConnectInfo;
            if (npgsqlInfo != null) {
                this.target = Target.PostgreSQL;
                this.info = info;
                return;
            }
            DaoOle.ConnectInfo oleInfo = info as DaoOle.ConnectInfo;
            if (oleInfo != null) {
                this.target = Target.OleDb;
                this.info = info;
                return;
            }
            DaoSQLite.ConnectInfo sqliteInfo = info as DaoSQLite.ConnectInfo;
            if (sqliteInfo != null) {
                this.target = Target.SQLite;
                this.info = info;
                return;
            }
        }

        /// <summary>
        /// DAO生成
        /// </summary>
        /// <returns>DAO</returns>
        public DaoBase Build()
        {
            DaoBase daoBase;
            switch (target) {
                case Target.SQLite:
                    daoBase = new DaoSQLite(this.info as DaoSQLite.ConnectInfo);
                    break;
                case Target.PostgreSQL:
                    daoBase = new DaoNpgsql(this.info as DaoNpgsql.ConnectInfo);
                    break;
                case Target.OleDb:
                    daoBase = new DaoOle(this.info as DaoOle.ConnectInfo);
                    break;
                default:
                    daoBase = null;
                    break;
            }
            return daoBase;
        }
    }
}
