using HouseholdAccountBook.Dao.Abstract;
using HouseholdAccountBook.DbHandler;
using HouseholdAccountBook.DbHandler.Abstract;
using System;
using static HouseholdAccountBook.Others.DbConstants;

namespace HouseholdAccountBook.DaoFactory.Abstract
{
    /// <summary>
    /// DAOファクトリのベースクラス
    /// </summary>
    /// <typeparam name="DAO"><see cref="ReadDaoBase"/>の派生クラス</typeparam>
    public abstract class ReadDaoFactoryBase<DAO> where DAO : ReadDaoBase
    {

        public ReadDaoFactoryBase() { }

        protected abstract DAO CreateSQLite(SQLiteDbHandler dbHandler);

        protected abstract DAO CreateNpgsql(NpgsqlDbHandler dbHandler);

        protected abstract DAO CreateAccess(OleDbHandler dbHandler);

        /// <summary>
        /// DBハンドラの接続先DBに基づいてDAOを生成する
        /// </summary>
        /// <param name="dbHandler">DBハンドラ</param>
        /// <returns>DAO</returns>
        /// <exception cref="NotSupportedException"></exception>
        public DAO Create(DbHandlerBase dbHandler, DBKind dbKind)
        {
            DAO dao = null;
            switch (dbHandler.LibKind) {
                case DBLibraryKind.SQLite:
                    switch (dbKind) {
                        case DBKind.SQLite:
                            dao = this.CreateSQLite(dbHandler as SQLiteDbHandler);
                            break;
                    }
                    break;
                case DBLibraryKind.PostgreSQL:
                    switch (dbKind) {
                        case DBKind.PostgreSQL:
                            dao = this.CreateNpgsql(dbHandler as NpgsqlDbHandler);
                            break;
                    }
                    break;
                case DBLibraryKind.OleDb:
                    switch (dbKind) {
                        case DBKind.Access:
                            dao = this.CreateAccess(dbHandler as OleDbHandler);
                            break;
                        default: // ここは選択肢が増えるかも
                            break;
                    }
                    break;
            }

            if (dao == null) {
                throw new NotSupportedException("Unsupported database type.");
            }

            return dao;
        }
    }
}
