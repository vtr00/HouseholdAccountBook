using HouseholdAccountBook.Adapters.Dto.Abstract;

namespace HouseholdAccountBook.Adapters.Dto.DbTable
{
    /// <summary>
    /// グループDTO
    /// </summary>
    public class HstGroupDto : TableDtoBase, ISequentialIDDto
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public HstGroupDto() { }

        public int GetId() => this.GroupId;

        /// <summary>
        /// グループID
        /// </summary>
        public int GroupId { get; set; }
        /// <summary>
        /// グループ種別
        /// </summary>
        public int GroupKind { get; set; }
        /// <summary>
        /// 備考(未使用)
        /// </summary>
        public string Remark { get; set; }
    }
}
