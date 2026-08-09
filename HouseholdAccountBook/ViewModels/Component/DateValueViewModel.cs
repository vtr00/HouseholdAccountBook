using HouseholdAccountBook.Models;
using HouseholdAccountBook.Models.AppServices;
using HouseholdAccountBook.Models.UiDto;
using HouseholdAccountBook.Models.ValueObjects;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.Views.UserControls;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HouseholdAccountBook.ViewModels.Component
{
    /// <summary>
    /// 日付金額VM
    /// </summary>
    public class DateValueViewModel : BindableBase, ILoadableAsync
    {
        #region プロパティ
        /// <summary>
        /// 帳簿項目ID
        /// </summary>
        public ActionIdObj ActionId { get; set; }

        /// <summary>
        /// 選択された日付
        /// </summary>
        public DateTime SelectedDate { get; set; }

        /// <summary>
        /// アセットセレクタVM
        /// </summary>
        public SelectorViewModel<AssetModel, AssetIdObj> AssetSelectorVM => field ??= new(static vm => vm?.Id);
        private void RaiseAssetChanged()
        {
            this.RaisePropertyChanged(nameof(this.InputedValueStr));
            this.RaisePropertyChanged(nameof(this.ValueScale));
        }

        /// <summary>
        /// 入力された金額(主単位)
        /// </summary>
        public decimal? InputedValue {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }
        /// <summary>
        /// 入力された金額(文字列)
        /// </summary>
        public string InputedValueStr => AssetService.Instance.ToAssetString(this.InputedValue, this.AssetSelectorVM.SelectedKey, UnitKind.MainUnit, UnitKind.MainUnit);
        /// <summary>
        /// 金額の小数点以下桁数
        /// </summary>
        public int ValueScale => AssetService.Instance.GetAssetModel(this.AssetSelectorVM.SelectedKey).Scale;

        /// <summary>
        /// <see cref="NumericUpDown"/> の編集状態
        /// </summary>
        /// <remarks>フィールドの初期状態は <see cref="NumericUpDown"/> に任せる</remarks>
        public NumericUpDown.EditSession Session {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        public void Initialize(Func<AssetIdObj> defaultSelector)
        {
            this.AssetSelectorVM.SetLoader(() => AssetService.Instance.Assets);
            this.AssetSelectorVM.SetDefaultSelector(defaultSelector);
        }

        public async Task LoadAsync(CancellationToken token = default)
        {
            await this.AssetSelectorVM.LoadAsync(token);
            this.RaiseAssetChanged();
        }

        public void AddEventHandlers() => this.AssetSelectorVM.SelectionChanged += (sender, e) => this.RaiseAssetChanged();
    }
}
