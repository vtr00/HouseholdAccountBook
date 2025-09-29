using HouseholdAccountBook.Adapters.Dto.Abstract;

namespace HouseholdAccountBook.Adapters.Dto.Others
{
    /// <summary>
    /// 分類-項目情報DTO
    /// </summary>
    public class CategoryItemInfoDto : DtoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CategoryItemInfoDto() : base() { }

        /// <summary>
        /// 項目ID
        /// </summary>
        public int ItemId { get; set; } = -1;
        /// <summary>
        /// 項目名
        /// </summary>
        public string ItemName { get; set; } = string.Empty;
        /// <summary>
        /// 分類名
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;
    }
}
