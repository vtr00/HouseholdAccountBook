using HouseholdAccountBook.Models.Dto.Abstract;

namespace HouseholdAccountBook.Models.Dto.Others
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
        public int? GroupId { get; set; } = null;
        /// <summary>
        /// グループ種別
        /// </summary>
        public int? GroupKind { get; set; } = null;
    }
}
