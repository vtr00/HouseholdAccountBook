using HouseholdAccountBook.Infrastructure.DB.DbDto.Abstract;

namespace HouseholdAccountBook.Infrastructure.DB.DbDto.DbTable
{
    /// <summary>
    /// アセットDTO
    /// </summary>
    public class MstAssetDto : MstDtoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MstAssetDto() : base() { }

        public override int GetId() => this.AssetId;

        /// <summary>
        /// アセットID
        /// </summary>
        public int AssetId { get; set; }

        /// <summary>
        /// アセットコード
        /// </summary>
        public string AssetCode { get; set; }

        /// <summary>
        /// アセット名
        /// </summary>
        public string AssetName { get; set; } = "(no name)";
        /// <summary>
        /// サブ単位名
        /// </summary>
        public string SubunitName { get; set; }

        /// <summary>
        /// アセット種別
        /// </summary>
        public int AssetKind { get; set; }

        /// <summary>
        /// 小数点以下桁数
        /// </summary>
        public int Scale { get; set; }

        /// <summary>
        /// 単位(前置)
        /// </summary>
        public string Prefix { get; set; } = string.Empty;
        /// <summary>
        /// 単位(後置)
        /// </summary>
        public string Suffix { get; set; } = string.Empty;
        /// <summary>
        /// サブ単位(前置)
        /// </summary>
        public string SubPrefix { get; set; }
        /// <summary>
        /// サブ単位(後置)
        /// </summary>
        public string SubSuffix { get; set; }

        /// <summary>
        /// 変換レート
        /// </summary>
        public decimal BaseRate { get; set; } = 1;
    }
}
