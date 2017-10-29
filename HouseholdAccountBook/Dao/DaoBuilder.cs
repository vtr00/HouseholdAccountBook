namespace HouseholdAccountBook.Dao
{
    /// <summary>
    /// DAOビルダ
    /// </summary>
    public partial class DaoBuilder
    {
        /// <summary>
        /// 接続対象
        /// </summary>
        private Target target;
        /// <summary>
        /// 接続情報
        /// </summary>
        private DaoBase.ConnectInfo info;

        /// <summary>
        /// <see cref="DaoBuilder"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="info">接続情報</param>
        public DaoBuilder(DaoBase.ConnectInfo info)
        {
            this.target = Target.Undefined;
            this.info = null;

            if (info is DaoNpgsql.ConnectInfo npgsqlInfo) {
                this.target = Target.PostgreSQL;
                this.info = info;
                return;
            }
            if (info is DaoOle.ConnectInfo oleInfo) {
                this.target = Target.OleDb;
                this.info = info;
                return;
            }
            if (info is DaoSQLite.ConnectInfo sqliteInfo) {
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
            switch (this.target) {
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
