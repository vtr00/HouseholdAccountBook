namespace HouseholdAccountBook.Dto
{
    /// <summary>
    /// グループ情報DTO
    /// </summary>
    public class GroupInfoDto
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public GroupInfoDto() { }

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
