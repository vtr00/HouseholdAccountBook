using HouseholdAccountBook.Extentions;
using Prism.Mvvm;
using System;
using static HouseholdAccountBook.ConstValue.ConstValue;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// VM
    /// </summary>
    public class TermWindowViewModel : BindableBase
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

        /// <summary>
        /// 選択された区間種別
        /// </summary>
        #region SelectedTermKind
        public TermKind SelectedTermKind
        {
            get {
                DateTime lastDate = this.StartDate.GetFirstDateOfMonth().AddMonths(1).AddMilliseconds(-1);
                if (this.StartDate.Day == 1 && DateTime.Equals(this.EndDate.Date, lastDate.Date)) {
                    return TermKind.Monthly;
                }
                else {
                    return TermKind.Selected;
                }
            }
        }
        #endregion
    }
}
