using HouseholdAccountBook.Adapters.Dto.Abstract;

namespace HouseholdAccountBook.Adapters.Dto.Others
{
    /// <summary>
    /// 帳簿-項目関連情報DTO
    /// </summary>
    public class ItemRelFromBookInfoDto : DtoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ItemRelFromBookInfoDto() : base() { }

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
        /// <summary>
        /// 収支種別
        /// </summary>
        public int BalanceKind { get; set; }
        /// <summary>
        /// 関連有無
        /// </summary>
        public bool IsRelated { get; set; }
    }
}
