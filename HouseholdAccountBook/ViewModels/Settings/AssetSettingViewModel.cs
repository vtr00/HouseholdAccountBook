using HouseholdAccountBook.Models;
using HouseholdAccountBook.Models.UiDto;
using HouseholdAccountBook.Models.ValueObjects;
using HouseholdAccountBook.ViewModels.Abstract;
using System.Collections.Generic;

namespace HouseholdAccountBook.ViewModels.Settings
{
    /// <summary>
    /// アセットVM(設定用)
    /// </summary>
    public class AssetSettingViewModel : BindableBase
    {
        /// <summary>
        /// アセットモデル
        /// </summary>
        public AssetModel Asset { get; init; }

        /// <summary>
        /// アセットID
        /// </summary>
        public AssetIdObj Id => this.Asset.Id;

        /// <summary>
        /// ソート順
        /// </summary>
        public int SortOrder => this.Asset.SortOrder;

        /// <summary>
        /// 入力されたアセット名
        /// </summary>
        public string InputedName { get; set; }

        /// <summary>
        /// 入力されたサブ単位名
        /// </summary>
        public string InputedSubunitName { get; set; }

        /// <summary>
        /// 入力されたアセットコード
        /// </summary>
        public string InputedAssetCode { get; set; }

        /// <summary>
        /// アセット種別セレクタ
        /// </summary>
        public SelectorViewModel<KeyValuePair<AssetKind, string>, AssetKind> AssetKindSelectorVM => field ??= new(static p => p.Key);

        /// <summary>
        /// 小数点以下桁数
        /// </summary>
        public int InputedScale { get; set; }
        
        /// <summary>
        /// 単位(前置)
        /// </summary>
        public string InputedPrefix { get; set; }
        /// <summary>
        /// 単位(後置)
        /// </summary>
        public string InputedSuffix { get; set; }
        /// <summary>
        /// サブ単位(前置)
        /// </summary>
        public string InputedSubPrefix { get; set; }
        /// <summary>
        /// サブ単位(後置)
        /// </summary>
        public string InputedSubSuffix { get; set; }

        /// <summary>
        /// 変換レート
        /// </summary>
        public decimal InputedBaseRate { get; set; } = 1;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="asset">アセットモデル</param>
        public AssetSettingViewModel(AssetModel asset)
        {
            this.Asset = asset;
            this.InputedName = asset.Name;
            this.InputedSubunitName = asset.SubunitName;
            this.InputedAssetCode = asset.AssetCode;
            this.InputedScale = asset.Scale;
            this.InputedPrefix = asset.Prefix;
            this.InputedSuffix = asset.Suffix;
            this.InputedSubPrefix = asset.SubPrefix;
            this.InputedSubSuffix = asset.SubSuffix;
            this.InputedBaseRate = asset.BaseRate;
        }
    }
}
