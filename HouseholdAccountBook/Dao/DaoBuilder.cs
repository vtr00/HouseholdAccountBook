﻿namespace HouseholdAccountBook.Dao
{
    /// <summary>
    /// DAOビルダ
    /// </summary>
    public partial class DaoBuilder
    {
        /// <summary>
        /// 接続対象
        /// </summary>
        private readonly Target target;
        /// <summary>
        /// 接続情報
        /// </summary>
        private readonly DaoBase.ConnectInfo info;

        /// <summary>
        /// <see cref="DaoBuilder"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="info">接続情報</param>
        public DaoBuilder(DaoBase.ConnectInfo info)
        {
            if (info is DaoNpgsql.ConnectInfo) {
                this.target = Target.PostgreSQL;
                this.info = info;
                return;
            }

            if (info is DaoOle.ConnectInfo) {
                this.target = Target.OleDb;
                this.info = info;
                return;
            }

            if (info is DaoSQLite.ConnectInfo) {
                this.target = Target.SQLite;
                this.info = info;
                return;
            }

            this.target = Target.Undefined;
            this.info = null;
        }

        /// <summary>
        /// DAO生成
        /// </summary>
        /// <returns>DAO</returns>
        public DaoBase Build()
        {
            try {
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
            catch (System.TimeoutException e) {
                throw e;
            }
        }
    }
}
