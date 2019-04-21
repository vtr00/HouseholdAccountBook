using HouseholdAccountBook.Extensions;
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

        /// <summary>
        /// 選択された区間種別
        /// </summary>
        #region SelectedTermKind
        public TermKind SelectedTermKind
        {
            get {
                DateTime lastDate = this.StartDate.GetFirstDateOfMonth().AddMonths(1).AddMilliseconds(-1);
                return (this.StartDate.Day == 1 && DateTime.Equals(this.EndDate.Date, lastDate.Date)) ? TermKind.Monthly : TermKind.Selected;
            }
        }
        #endregion
    }
}
