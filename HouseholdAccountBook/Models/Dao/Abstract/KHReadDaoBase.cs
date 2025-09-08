using HouseholdAccountBook.DbHandler;
using HouseholdAccountBook.Models.Dto.Abstract;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Models.Dao.Abstract
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
        protected readonly OleDbHandler dbHandler = dbHandler;

        public abstract Task<IEnumerable<DTO>> FindAllAsync();
    }
}
