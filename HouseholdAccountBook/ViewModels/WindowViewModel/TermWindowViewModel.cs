using Prism.Mvvm;
using System;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// 期間指定ウィンドウVM
    /// </summary>
    public class TermWindowViewModel : BindableBase
    {
        /// <summary>
        /// 開始日
        /// </summary>
        #region StartDate
        public DateTime StartDate
        {
            get => this._StartDate;
            set => this.SetProperty(ref this._StartDate, value);
        }
        private DateTime _StartDate = DateTime.Now;
        #endregion

        /// <summary>
        /// 終了日
        /// </summary>
        #region EndDate
        public DateTime EndDate
        {
            get => this._EndDate;
            set => this.SetProperty(ref this._EndDate, value);
        }
        private DateTime _EndDate = DateTime.Now;
        #endregion
    }
}
