using HouseholdAccountBook.Adapters.DbHandlers;
using HouseholdAccountBook.Adapters.Dto.Abstract;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Adapters.Dao.Abstract
{
    /// <summary>
    /// 記帳風月のDTO向けのDAOのベースクラス(読み込み専用)
    /// </summary>
    /// <typeparam name="DTO"><see cref="KHDtoBase"/>の派生クラス</typeparam>
    /// <param name="dbHandler">Ole DBハンドラ</param>
    public abstract class KHReadDaoBase<DTO>(OleDbHandler dbHandler) : IReadTableDao<DTO> where DTO : KHDtoBase
    {
        /// <summary>
        /// Ole DBハンドラ
        /// </summary>
        protected readonly OleDbHandler mDbHandler = dbHandler;

        public abstract Task<IEnumerable<DTO>> FindAllAsync();
    }
}
