using HouseholdAccountBook.Adapters.DbHandlers.Abstract;

namespace HouseholdAccountBook.Adapters.Dao.Abstract
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
