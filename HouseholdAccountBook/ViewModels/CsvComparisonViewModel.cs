using Prism.Mvvm;
using System;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// CSV比較VM
    /// </summary>
    public class CsvComparisonViewModel : BindableBase
    {
        /// <summary>
        /// 帳簿項目ID
        /// </summary>
        #region ActionId
        public int? ActionId
        {
            get { return _ActionId; }
            set { SetProperty(ref _ActionId, value); }
        }
        private int? _ActionId = default(int?);
        #endregion

        /// <summary>
        /// 項目名
        /// </summary>
        #region ItemName
        public string ItemName
        {
            get { return _ItemName; }
            set { SetProperty(ref _ItemName, value); }
        }
        private string _ItemName = default(string);
        #endregion

        /// <summary>
        /// 店舗名
        /// </summary>
        #region ShopName
        public string ShopName
        {
            get { return _ShopName; }
            set { SetProperty(ref _ShopName, value); }
        }
        private string _ShopName = default(string);
        #endregion

        /// <summary>
        /// 備考
        /// </summary>
        #region Remark
        public string Remark
        {
            get { return _Remark; }
            set { SetProperty(ref _Remark, value); }
        }
        private string _Remark = default(string);
        #endregion

        /// <summary>
        /// 一致するか
        /// </summary>
        #region IsMatch
        public bool IsMatch
        {
            get { return _IsMatch; }
            set { SetProperty(ref _IsMatch, value); }
        }
        private bool _IsMatch = default(bool);
        #endregion

        /// <summary>
        /// CSVデータ
        /// </summary>
        #region Record
        public CsvRecord Record
        {
            get { return _Record; }
            set { SetProperty(ref _Record, value); }
        }
        private CsvRecord _Record = default(CsvRecord);
        #endregion

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
