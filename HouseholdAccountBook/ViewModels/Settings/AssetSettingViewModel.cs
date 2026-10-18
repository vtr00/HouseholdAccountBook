using HouseholdAccountBook.Models;
using HouseholdAccountBook.Models.UiDto;
using HouseholdAccountBook.Models.ValueObjects;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Common;
using System.Collections.Generic;

namespace HouseholdAccountBook.ViewModels.Settings
{
    /// <summary>
    /// アセットVM(設定用)
    /// </summary>
    public class AssetSettingViewModel : ViewModelBase
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
        /// アセット名
        /// </summary>
        public InputableViewModel<string> NameVM { get; } = new();
        /// <summary>
        /// サブ単位名
        /// </summary>
        public InputableViewModel<string> SubunitNameVM { get; } = new();

        /// <summary>
        /// アセットコード
        /// </summary>
        public InputableViewModel<string> AssetCodeVM { get; } = new();
        /// <summary>
        /// アセット種別セレクタ
        /// </summary>
        public SelectableViewModel<KeyValuePair<AssetKind, string>, AssetKind> AssetKindListVM => field ??= new(static p => p.Key);

        /// <summary>
        /// 小数点以下桁数
        /// </summary>
        public InputableViewModel<int> ScaleVM { get; } = new();
        /// <summary>
        /// 単位(前置)
        /// </summary>
        public InputableViewModel<string> PrefixVM { get; } = new();
        /// <summary>
        /// 単位(後置)
        /// </summary>
        public InputableViewModel<string> SuffixVM { get; } = new();
        /// <summary>
        /// サブ単位(前置)
        /// </summary>
        public InputableViewModel<string> SubPrefixVM { get; } = new();
        /// <summary>
        /// サブ単位(後置)
        /// </summary>
        public InputableViewModel<string> SubSuffixVM { get; } = new();

        /// <summary>
        /// 変換レート
        /// </summary>
        public InputableViewModel<decimal> BaseRateVM { get; } = new();

        /// <summary>
        /// 未保存状態マネージャ
        /// </summary>
        public DirtyStatusManager DirtyState => field ??=
            new(this.NameVM, this.SubunitNameVM, this.AssetCodeVM, this.AssetKindListVM, this.ScaleVM, this.PrefixVM, this.SuffixVM,
                this.SubPrefixVM, this.SubSuffixVM, this.BaseRateVM);

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="asset">アセットモデル</param>
        public AssetSettingViewModel(AssetModel asset)
        {
            this.Asset = asset;
            this.NameVM.LoadedValue = asset.Name;
            this.SubunitNameVM.LoadedValue = asset.SubunitName;
            this.AssetCodeVM.LoadedValue = asset.AssetCode;
            this.ScaleVM.LoadedValue = asset.Scale;
            this.PrefixVM.LoadedValue = asset.Prefix;
            this.SuffixVM.LoadedValue = asset.Suffix;
            this.SubPrefixVM.LoadedValue = asset.SubPrefix;
            this.SubSuffixVM.LoadedValue = asset.SubSuffix;
            this.BaseRateVM.LoadedValue = asset.BaseRate;
        }
    }
}
