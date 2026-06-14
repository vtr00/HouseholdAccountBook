using HouseholdAccountBook.Models.ValueObjects;

namespace HouseholdAccountBook.Models.UiDto
{
    /// <summary>
    /// アセットModel
    /// </summary>
    public class AssetModel(AssetIdObj id, string name)
    {
        #region プロパティ
        /// <summary>
        /// アセットID
        /// </summary>
        public AssetIdObj Id { get; init; } = id;

        /// <summary>
        /// ソート順
        /// </summary>
        public int SortOrder { get; init; }

        /// <summary>
        /// アセット名
        /// </summary>
        public string Name { get; init; } = name;
        /// <summary>
        /// サブ単位名
        /// </summary>
        public string SubunitName { get; init; }

        /// <summary>
        /// アセットコード
        /// </summary>
        public string AssetCode { get; init; }
        /// <summary>
        /// アセット種別
        /// </summary>
        public AssetKind AssetKind { get; init; }

        /// <summary>
        /// 小数点以下桁数
        /// </summary>
        public int Scale { get; init; }

        /// <summary>
        /// 単位(前置)
        /// </summary>
        public string Prefix { get; init; }
        /// <summary>
        /// 単位(後置)
        /// </summary>
        public string Suffix { get; init; }
        /// <summary>
        /// サブ単位(前置)
        /// </summary>
        public string SubPrefix { get; init; }
        /// <summary>
        /// サブ単位(後置)
        /// </summary>
        public string SubSuffix { get; init; }

        /// <summary>
        /// 変換レート
        /// </summary>
        public decimal BaseRate { get; init; }

        /// <summary>
        /// デフォルトアセットか
        /// </summary>
        public bool IsDefault { get; init; }
        #endregion
    }
}
