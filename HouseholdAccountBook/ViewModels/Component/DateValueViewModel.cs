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
        /// 日付
        /// </summary>
        public DateTime ActDate { get; set; }

        /// <summary>
        /// 金額(主単位)
        /// </summary>
        public decimal? ActValue {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }
        /// <summary>
        /// 金額(文字列)
        /// </summary>
        public string ActValueStr => AssetService.Instance.ToAssetString(this.ActValue, null, UnitKind.MainUnit, UnitKind.MainUnit);
        /// <summary>
        /// 小数点以下桁数
        /// </summary>
        public int Scale {
            get;
            set => this.SetProperty(ref field, value);
        }

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
