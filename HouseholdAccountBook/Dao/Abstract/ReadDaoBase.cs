using HouseholdAccountBook.DbHandler.Abstract;

namespace HouseholdAccountBook.Dao.Abstract
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
