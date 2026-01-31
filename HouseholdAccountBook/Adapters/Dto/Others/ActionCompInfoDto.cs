using HouseholdAccountBook.Adapters.Dto.Abstract;

namespace HouseholdAccountBook.Adapters.Dto.Others
{
    /// <summary>
    /// 帳簿項目比較情報DTO
    /// </summary>
    public class ActionCompInfoDto : VirTableDtoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ActionCompInfoDto() : base() { }

        /// <summary>
        /// 帳簿ID
        /// </summary>
        public int ActionId { get; set; } = -1;
        /// <summary>
        /// 項目名
        /// </summary>
        public string ItemName { get; set; } = string.Empty;
        /// <summary>
        /// 支出
        /// </summary>
        public int ActValue { get; set; }
        /// <summary>
        /// 店舗名
        /// </summary>
        public string ShopName { get; set; }
        /// <summary>
        /// 備考
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 一致フラグ
        /// </summary>
        public int IsMatch { get; set; }
        /// <summary>
        /// グループID
        /// </summary>
        public int? GroupId { get; set; }
    }
}
