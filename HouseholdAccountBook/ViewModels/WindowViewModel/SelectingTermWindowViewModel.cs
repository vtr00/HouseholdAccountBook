using Prism.Mvvm;
using System;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// VM
    /// </summary>
    public class SelectingTermWindowViewModel : BindableBase
    {
        /// <summary>
        /// 開始日
        /// </summary>
        #region StartDate
        public DateTime StartDate
        {
            get { return this._StartDate; }
            set { SetProperty(ref this._StartDate, value); }
        }
        private DateTime _StartDate = DateTime.Now;
        #endregion

        /// <summary>
        /// 終了日
        /// </summary>
        #region EndDate
        public DateTime EndDate
        {
            get { return this._EndDate; }
            set { SetProperty(ref this._EndDate, value); }
        }
        private DateTime _EndDate = DateTime.Now;
        #endregion
    }
}
