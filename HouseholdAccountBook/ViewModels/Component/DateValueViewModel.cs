using HouseholdAccountBook.ViewModels.Abstract;
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
        public int? ActionId { get; set; }

        /// <summary>
        /// 日付
        /// </summary>
        public DateTime ActDate { get; set; }

        /// <summary>
        /// 金額
        /// </summary>
        #region ActValue
        public int? ActValue
        {
            get => this._ActValue;
            set {
                if (this.SetProperty(ref this._ActValue, value)) {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }
        private int? _ActValue = default;
        #endregion
        #endregion
    }
}
