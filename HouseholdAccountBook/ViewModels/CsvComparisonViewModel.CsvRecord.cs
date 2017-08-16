using Prism.Mvvm;
using System;

namespace HouseholdAccountBook.ViewModels
{
    public partial class CsvComparisonViewModel : BindableBase
    {
        /// <summary>
        /// CSVレコード
        /// </summary>
        public class CsvRecord : BindableBase
        {
            /// <summary>
            /// 日付
            /// </summary>
            #region Date
            public DateTime Date
            {
                get { return _Date; }
                set { SetProperty(ref _Date, value); }
            }
            private DateTime _Date = default(DateTime);
            #endregion

            /// <summary>
            /// 値
            /// </summary>
            #region Value
            public int Value
            {
                get { return _Value; }
                set { SetProperty(ref _Value, value); }
            }
            private int _Value = default(int);
            #endregion

            /// <summary>
            /// 名称
            /// </summary>
            #region Name
            public string Name
            {
                get { return _Name; }
                set { SetProperty(ref _Name, value); }
            }
            private string _Name = default(string);
            #endregion
        }
    }
}
