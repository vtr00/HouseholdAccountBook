using HouseholdAccountBook.Infrastructure.DB.DbDto.Abstract;

namespace HouseholdAccountBook.Infrastructure.DB.DbDto.Others
{
    /// <summary>
    /// グループ情報DTO
    /// </summary>
    public class GroupInfoDto : VirTableDtoBase
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
