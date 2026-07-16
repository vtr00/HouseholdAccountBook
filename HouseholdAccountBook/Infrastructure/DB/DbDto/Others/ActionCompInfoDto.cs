using HouseholdAccountBook.Infrastructure.DB.DbDto.Abstract;
using System;

namespace HouseholdAccountBook.Infrastructure.DB.DbDto.Others
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
        /// 項目日時
        /// </summary>
        public DateTime ActTime { get; set; } = DateTime.Now;
        /// <summary>
        /// 項目ID
        /// </summary>
        public int ItemId { get; set; } = -1;
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
        /// 表示アセットでの支出(主単位)
        /// </summary>
        public decimal MainActValue { get; set; }
        /// <summary>
        /// 表示アセットでのアセットID
        /// </summary>
        public int ActAssetId { get; set; }
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
