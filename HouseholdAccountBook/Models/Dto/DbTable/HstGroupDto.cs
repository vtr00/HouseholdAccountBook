using HouseholdAccountBook.Models.Dto.Abstract;

namespace HouseholdAccountBook.Models.Dto.DbTable
{
    /// <summary>
    /// グループテーブルDTO
    /// </summary>
    public class HstGroupDto : TableDtoBase, ISequentialIDDto
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public HstGroupDto() { }

        public int GetId()
        {
            return this.GroupId;
        }

        /// <summary>
        /// グループID
        /// </summary>
        public int GroupId { get; set; } = 0;
        /// <summary>
        /// グループ種別
        /// </summary>
        public int GroupKind { get; set; } = 0;
        /// <summary>
        /// 備考(未使用)
        /// </summary>
        public string Remark { get; set; } = null;
    }
}
