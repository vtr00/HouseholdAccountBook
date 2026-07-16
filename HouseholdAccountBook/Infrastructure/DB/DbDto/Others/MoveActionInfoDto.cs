using HouseholdAccountBook.Infrastructure.DB.DbDto.Abstract;
using System;

namespace HouseholdAccountBook.Infrastructure.DB.DbDto.Others
{
    /// <summary>
    /// 移動情報DTO
    /// </summary>
    public class MoveActionInfoDto : VirTableDtoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MoveActionInfoDto() : base() { }

        /// <summary>
        /// 帳簿ID
        /// </summary>
        public int BookId { get; set; } = -1;
        /// <summary>
        /// 帳簿項目ID
        /// </summary>
        public int ActionId { get; set; } = -1;
        /// <summary>
        /// 項目ID
        /// </summary>
        public int ItemId { get; set; } = -1;
        /// <summary>
        /// 項目日時
        /// </summary>
        public DateTime ActTime { get; set; } = DateTime.Now;
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
        /// 表示アセットでの項目値(主単位)
        /// </summary>
        public decimal MainActValue { get; set; }
        /// <summary>
        /// 表示アセットでのアセットID
        /// </summary>
        public int ActAssetId { get; set; }
        /// <summary>
        /// 備考
        /// </summary>
        public string Remark { get; set; } = string.Empty;
        /// <summary>
        /// 移動フラグ
        /// </summary>
        public int MoveFlg { get; set; }
    }
}
