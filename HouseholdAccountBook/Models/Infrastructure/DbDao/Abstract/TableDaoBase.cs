using HouseholdAccountBook.Models.Infrastructure.DbHandlers.Abstract;

namespace HouseholdAccountBook.Models.Infrastructure.DbDao.Abstract
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
