using HouseholdAccountBook.Models.DomainModels;
using HouseholdAccountBook.Models.Infrastructure;
using HouseholdAccountBook.Models.Utilities.Args;
using HouseholdAccountBook.ViewModels.Abstract;
using System;

namespace HouseholdAccountBook.ViewModels.Component
{
    /// <summary>
    /// CSV比較VM
    /// </summary>
    public class CsvComparisonViewModel : BindableBase, ISelectable
    {
        #region イベント
        /// <summary>
        /// 一致フラグ変更時のイベント
        /// </summary>
        public event Action<EventArgs<int?, bool>> IsMatchChanged;
        #endregion

        #region プロパティ
        /// <summary>
        /// 一致フラグ
        /// </summary>
        #region IsMatch
        public bool IsMatch {
            get;
            set {
                if (this.Action is not null) {
                    if (this.SetProperty(ref field, value)) {
                        this.IsMatchChanged?.Invoke(new EventArgs<int?, bool>(this.Action.ActionId, value));
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// CSVデータ
        /// </summary>
        #region Record
        public ActionCsvDto Record {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        /// <summary>
        /// 帳簿項目
        /// </summary>
        #region Action
        public ActionModel Action {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 選択フラグ
        /// </summary>
        #region IsSelected
        public bool SelectFlag {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        #endregion

        /// <summary>
        /// 帳簿項目情報をクリアする
        /// </summary>
        public void ClearActionInfo()
        {
            this.Action = default;
            this.IsMatch = default;
        }
    }
}
