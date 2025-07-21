using HouseholdAccountBook.DbHandler.Abstract;

namespace HouseholdAccountBook.Dao.Abstract
{
    /// <summary>
    /// 読み込み専用のDTO向けのDAOのベースクラス
    /// </summary>
    public abstract class ReadDaoBase
    {
        /// <summary>
        /// DBハンドラ
        /// </summary>
        protected readonly DbHandlerBase dbHandler;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dbHandler">DBハンドラ</param>
        public ReadDaoBase(DbHandlerBase dbHandler)
        {
            this.dbHandler = dbHandler;
        }
    }
}
