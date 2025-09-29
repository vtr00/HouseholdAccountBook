using HouseholdAccountBook.Adapters.DbHandler.Abstract;

namespace HouseholdAccountBook.Adapters.Dao.Abstract
{
    /// <summary>
    /// 読み込み専用のDTO向けのDAOのベースクラス
    /// </summary>
    /// <param name="dbHandler">DBハンドラ</param>
    public abstract class ReadDaoBase(DbHandlerBase dbHandler)
    {
        /// <summary>
        /// DBハンドラ
        /// </summary>
        protected readonly DbHandlerBase dbHandler = dbHandler;
    }
}
