namespace HouseholdAccountBook.Dto
{
    /// <summary>
    /// 帳簿項目比較情報DTO
    /// </summary>
    public class ActionCompInfoDto
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ActionCompInfoDto() { }

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
        public int ActValue { get; set; } = 0;
        /// <summary>
        /// 店舗名
        /// </summary>
        public string ShopName { get; set; } = string.Empty;
        /// <summary>
        /// 備考
        /// </summary>
        public string Remark { get; set; } = string.Empty;
        /// <summary>
        /// 一致フラグ
        /// </summary>
        public int IsMatch { get; set; } = 0;
        /// <summary>
        /// グループID
        /// </summary>
        public int GroupId { get; set; } = -1;
    }
}
