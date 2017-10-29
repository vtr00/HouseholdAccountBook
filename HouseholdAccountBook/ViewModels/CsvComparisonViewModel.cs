using Prism.Mvvm;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// CSV比較VM
    /// </summary>
    public partial class CsvComparisonViewModel : BindableBase
    {
        #region プロパティ
        /// <summary>
        /// 帳簿項目ID
        /// </summary>
        #region ActionId
        public int? ActionId
        {
            get { return this._ActionId; }
            set { SetProperty(ref this._ActionId, value); }
        }
        private int? _ActionId = default(int?);
        #endregion

        /// <summary>
        /// 項目名
        /// </summary>
        #region ItemName
        public string ItemName
        {
            get { return this._ItemName; }
            set { SetProperty(ref this._ItemName, value); }
        }
        private string _ItemName = default(string);
        #endregion

        /// <summary>
        /// 店舗名
        /// </summary>
        #region ShopName
        public string ShopName
        {
            get { return this._ShopName; }
            set { SetProperty(ref this._ShopName, value); }
        }
        private string _ShopName = default(string);
        #endregion

        /// <summary>
        /// 備考
        /// </summary>
        #region Remark
        public string Remark
        {
            get { return this._Remark; }
            set { SetProperty(ref this._Remark, value); }
        }
        private string _Remark = default(string);
        #endregion

        /// <summary>
        /// 一致するか
        /// </summary>
        #region IsMatch
        public bool IsMatch
        {
            get { return this._IsMatch; }
            set { SetProperty(ref this._IsMatch, value); }
        }
        private bool _IsMatch = default(bool);
        #endregion

        /// <summary>
        /// CSVデータ
        /// </summary>
        #region Record
        public CsvRecord Record
        {
            get { return this._Record; }
            set { SetProperty(ref this._Record, value); }
        }
        private CsvRecord _Record = default(CsvRecord);
        #endregion
        #endregion

        /// <summary>
        /// 帳簿項目情報をクリアする
        /// </summary>
        public void ClearActionInfo()
        {
            this.ActionId = default(int?);
            this.ItemName = default(string);
            this.ShopName = default(string);
            this.Remark = default(string);
            this.IsMatch = default(bool);
        }
    }
}
