using HouseholdAccountBook.Models;
using HouseholdAccountBook.Models.AppServices;
using HouseholdAccountBook.Models.ValueObjects;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.Views.UserControls;
using System;
using System.Windows.Input;

namespace HouseholdAccountBook.ViewModels.Component
{
    /// <summary>
    /// 日付金額VM
    /// </summary>
    public class DateValueViewModel : BindableBase
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
        /// 選択されたアセットID
        /// </summary>
        public AssetIdObj SelectedAssetId => this.SelectedAccountAssetId; //TODO: 帳簿項目のアセットIDがSystemでなければ帳簿のアセットIDを採用する
        /// <summary>
        /// 選択された帳簿のアセットID
        /// </summary>
        public AssetIdObj SelectedAccountAssetId {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    this.RaisePropertyChanged(nameof(this.InputedValueStr));
                    this.RaisePropertyChanged(nameof(this.ValueScale));
                }
            }
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
        public string InputedValueStr => AssetService.Instance.ToAssetString(this.InputedValue, this.SelectedAssetId, UnitKind.MainUnit, UnitKind.MainUnit);
        /// <summary>
        /// 金額の小数点以下桁数
        /// </summary>
        public int ValueScale => AssetService.Instance.GetAssetModel(this.SelectedAssetId).Scale;

        /// <summary>
        /// <see cref="NumericUpDown"/> の編集状態
        /// </summary>
        /// <remarks>フィールドの初期状態は <see cref="NumericUpDown"/> に任せる</remarks>
        public NumericUpDown.EditSession Session {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
    }
}
