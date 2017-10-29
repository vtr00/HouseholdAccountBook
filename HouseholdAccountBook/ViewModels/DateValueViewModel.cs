using Prism.Mvvm;
using System;
using System.Windows.Input;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// 日付金額VM
    /// </summary>
    public class DateValueViewModel : BindableBase
    {
        #region プロパティ
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
            get { return this._ActValue; }
            set {
                if (SetProperty(ref this._ActValue, value)) {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }
        private int? _ActValue = default(int?);
        #endregion
        #endregion
    }
}
