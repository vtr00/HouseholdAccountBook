using HouseholdAccountBook.Infrastructure.DB.DbDto.Abstract;
using System;

namespace HouseholdAccountBook.Infrastructure.DB.DbDto.Others
{
    /// <summary>
    /// 帳簿項目情報DTO
    /// </summary>
    public class ActionInfoDto : VirTableDtoBase
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
        /// 帳簿項目のアセットID
        /// </summary>
        public int? AssetId { get; set; }
        /// <summary>
        /// 帳簿項目アセットでの項目値(主単位)
        /// </summary>
        public decimal OrgMainActValue { get; set; }
        /// <summary>
        /// 帳簿項目アセットでのアセットID
        /// </summary>
        public int OrgActAssetId { get; set; }
        /// <summary>
        /// デフォルトまたは帳簿アセットでの項目値(主単位)
        /// </summary>
        public decimal MainActValue { get; set; }
        /// <summary>
        /// デフォルトまたは帳簿アセットでのアセットID
        /// </summary>
        public int ActAssetId { get; set; }
        /// <summary>
        /// 店舗名
        /// </summary>
        public string ShopName { get; set; } = string.Empty;
        /// <summary>
        /// グループID
        /// </summary>
        public int? GroupId { get; set; }
        /// <summary>
        /// 備考
        /// </summary>
        public string Remark { get; set; } = string.Empty;
        /// <summary>
        /// 一致フラグ
        /// </summary>
        public int IsMatch { get; set; }
    }
}
