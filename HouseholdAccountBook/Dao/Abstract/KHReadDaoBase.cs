using HouseholdAccountBook.DbHandler;
using HouseholdAccountBook.Dto.Abstract;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Dao.Abstract
{
    /// <summary>
    /// 記帳風月のDTO向けのDAOのベースクラス(読み込み専用)
    /// </summary>
    /// <typeparam name="DTO"><see cref="KHDtoBase"/>の派生クラス</typeparam>
    /// <param name="dbHandler">Ole DBハンドラ</param>
    public abstract class KHReadDaoBase<DTO>(OleDbHandler dbHandler) where DTO : KHDtoBase
    {
        /// <summary>
        /// Ole DBハンドラ
        /// </summary>
        protected readonly OleDbHandler dbHandler = dbHandler;

        /// <summary>
        /// <see cref="DTO"/> の全てのレコードを取得する
        /// </summary>
        /// <returns>DTOリスト</returns>
        public abstract Task<IEnumerable<DTO>> FindAllAsync();
    }
}
