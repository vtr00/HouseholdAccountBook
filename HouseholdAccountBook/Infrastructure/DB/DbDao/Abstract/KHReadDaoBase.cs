using HouseholdAccountBook.Infrastructure.DB.DbDto.Abstract;
using HouseholdAccountBook.Models.DbHandlers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Infrastructure.DB.DbDao.Abstract
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
