using HouseholdAccountBook.Adapters.Dto.Abstract;

namespace HouseholdAccountBook.Adapters.Dto.Others
{
    /// <summary>
    /// グループ情報DTO
    /// </summary>
    public class GroupInfoDto : DtoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public GroupInfoDto() : base() { }

        /// <summary>
        /// グループID
        /// </summary>
        public int? GroupId { get; set; }
        /// <summary>
        /// グループ種別
        /// </summary>
        public int? GroupKind { get; set; }
    }
}
