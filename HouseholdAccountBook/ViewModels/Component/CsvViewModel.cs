using HouseholdAccountBook.ViewModels.Abstract;
using System;

namespace HouseholdAccountBook.ViewModels.Component
{
    /// <summary>
    /// CSV VM
    /// </summary>
    public class CsvViewModel : BindableBase
    {
        #region プロパティ
        /// <summary>
        /// 日付
        /// </summary>
        #region Date
        public DateTime Date {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 値
        /// </summary>
        #region Value
        public int Value {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 名称
        /// </summary>
        #region Name
        public string Name {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        #endregion
    }
}
