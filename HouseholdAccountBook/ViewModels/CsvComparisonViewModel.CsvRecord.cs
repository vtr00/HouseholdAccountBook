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
                get => this._Date;
                set => this.SetProperty(ref this._Date, value);
            }
            private DateTime _Date = default;
            #endregion

            /// <summary>
            /// 値
            /// </summary>
            #region Value
            public int Value
            {
                get => this._Value;
                set => this.SetProperty(ref this._Value, value);
            }
            private int _Value = default;
            #endregion

            /// <summary>
            /// 名称
            /// </summary>
            #region Name
            public string Name
            {
                get => this._Name;
                set => this.SetProperty(ref this._Name, value);
            }
            private string _Name = default;
            #endregion
        }
    }
}
