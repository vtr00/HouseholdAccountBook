using HouseholdAccountBook.Adapters.Dto.Abstract;
using System;

namespace HouseholdAccountBook.Adapters.Dto.Others
{
    /// <summary>
    /// 帳簿項目情報DTO
    /// </summary>
    public class ActionInfoDto : DtoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ActionInfoDto() : base() { }

        /// <summary>
        /// 帳簿項目ID
        /// </summary>
        public int ActionId { get; set; } = -1;
        /// <summary>
        /// 項目日時
        /// </summary>
        public DateTime ActTime { get; set; } = DateTime.Now;
        /// <summary>
        /// 帳簿ID
        /// </summary>
        public int BookId { get; set; } = -1;
        /// <summary>
        /// 分類ID
        /// </summary>
        public int CategoryId { get; set; } = -1;
        /// <summary>
        /// 項目ID
        /// </summary>
        public int ItemId { get; set; } = -1;
        /// <summary>
        /// 帳簿名
        /// </summary>
        public string BookName { get; set; } = string.Empty;
        /// <summary>
        /// 分類名
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;
        /// <summary>
        /// 項目名
        /// </summary>
        public string ItemName { get; set; } = string.Empty;
        /// <summary>
        /// 項目値
        /// </summary>
        public int ActValue { get; set; } = 0;
        /// <summary>
        /// 店舗名
        /// </summary>
        public string ShopName { get; set; } = string.Empty;
        /// <summary>
        /// グループID
        /// </summary>
        public int? GroupId { get; set; } = null;
        /// <summary>
        /// 備考
        /// </summary>
        public string Remark { get; set; } = string.Empty;
        /// <summary>
        /// 一致フラグ
        /// </summary>
        public int IsMatch { get; set; } = 0;
    }
}
