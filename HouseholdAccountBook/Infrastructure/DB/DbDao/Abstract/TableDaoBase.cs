using HouseholdAccountBook.Infrastructure.DB.DbHandlers.Abstract;

namespace HouseholdAccountBook.Infrastructure.DB.DbDao.Abstract
{
    /// <summary>
    /// DB DAOのベースクラス
    /// </summary>
    /// <param name="dbHandler">DBハンドラ</param>
    public abstract class TableDaoBase(DbHandlerBase dbHandler)
    {
        /// <summary>
        /// DBハンドラ
        /// </summary>
        protected readonly DbHandlerBase mDbHandler = dbHandler;
    }
}
