using HouseholdAccountBook.DbHandler.Abstract;
using HouseholdAccountBook.Dto.Abstract;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Dao.Abstract
{
    /// <summary>
    /// 読み書き可能なDTO向けのDAOのベースクラス
    /// </summary>
    /// <typeparam name="DTO"><see cref="TableDtoBase"/>の派生クラス</typeparam>
    public abstract class ReadWriteDaoBase<DTO> : ReadDaoBase where DTO : TableDtoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dbHandler">DBハンドラ</param>
        public ReadWriteDaoBase(DbHandlerBase dbHandler) : base(dbHandler) { }

        /// <summary>
        /// テーブルの全てのレコードを取得する
        /// </summary>
        /// <returns>DTOリスト</returns>
        public abstract Task<IEnumerable<DTO>> FindAllAsync();

        /// <summary>
        /// レコードを挿入する
        /// </summary>
        /// <param name="dto">DTO</param>
        /// <returns>挿入行数</returns>
        public abstract Task<int> InsertAsync(DTO dto);

        /// <summary>
        /// レコードを更新する
        /// </summary>
        /// <param name="dto">DTO</param>
        /// <returns>更新行数</returns>
        public abstract Task<int> UpdateAsync(DTO dto);

        /// <summary>
        /// レコードを挿入または更新する
        /// </summary>
        /// <param name="dto">DTO</param>
        /// <returns>挿入/更新行数</returns>
        public abstract Task<int> UpsertAsync(DTO dto);

        /// <summary>
        /// テーブルの全てのレコードを削除する
        /// </summary>
        /// <returns>削除行数</returns>
        public abstract Task<int> DeleteAllAsync();
    }
}
