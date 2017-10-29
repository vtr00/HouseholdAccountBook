﻿using Prism.Mvvm;
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
                get { return this._Date; }
                set { SetProperty(ref this._Date, value); }
            }
            private DateTime _Date = default(DateTime);
            #endregion

            /// <summary>
            /// 値
            /// </summary>
            #region Value
            public int Value
            {
                get { return this._Value; }
                set { SetProperty(ref this._Value, value); }
            }
            private int _Value = default(int);
            #endregion

            /// <summary>
            /// 名称
            /// </summary>
            #region Name
            public string Name
            {
                get { return this._Name; }
                set { SetProperty(ref this._Name, value); }
            }
            private string _Name = default(string);
            #endregion
        }
    }
}
