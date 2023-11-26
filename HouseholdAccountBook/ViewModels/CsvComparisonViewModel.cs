using HouseholdAccountBook.Interfaces;
using HouseholdAccountBook.UserEventArgs;
using Prism.Mvvm;
using System;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// CSV比較VM
    /// </summary>
    public partial class CsvComparisonViewModel : BindableBase, IMultiSelectable
    {
        #region フィールド
        /// <summary>
        /// 一致フラグ変更時のイベント
        /// </summary>
        public event Action<EventArgs<int?, bool>> IsMatchChanged;
        #endregion

        #region プロパティ
        /// <summary>
        /// 帳簿項目ID
        /// </summary>
        #region ActionId
        public int? ActionId
        {
            get => this._ActionId;
            set => this.SetProperty(ref this._ActionId, value);
        }
        private int? _ActionId = default;
        #endregion

        /// <summary>
        /// 項目名
        /// </summary>
        #region ItemName
        public string ItemName
        {
            get => this._ItemName;
            set => this.SetProperty(ref this._ItemName, value);
        }
        private string _ItemName = default;
        #endregion

        /// <summary>
        /// 店舗名
        /// </summary>
        #region ShopName
        public string ShopName
        {
            get => this._ShopName;
            set => this.SetProperty(ref this._ShopName, value);
        }
        private string _ShopName = default;
        #endregion

        /// <summary>
        /// 備考
        /// </summary>
        #region Remark
        public string Remark
        {
            get => this._Remark;
            set => this.SetProperty(ref this._Remark, value);
        }
        private string _Remark = default;
        #endregion

        /// <summary>
        /// 一致フラグ
        /// </summary>
        #region IsMatch
        public bool IsMatch
        {
            get => this._IsMatch;
            set {
                if (this.ActionId.HasValue) {
                    if (this.SetProperty(ref this._IsMatch, value)) {
                        this.IsMatchChanged?.Invoke(new EventArgs<int?, bool>(this.ActionId, value));
                    }
                }
            }
        }
        private bool _IsMatch = default;
        #endregion

        /// <summary>
        /// CSVデータ
        /// </summary>
        #region Record
        public CsvRecord Record
        {
            get => this._Record;
            set => this.SetProperty(ref this._Record, value);
        }
        private CsvRecord _Record = default;
        #endregion

        /// <summary>
        /// 選択フラグ
        /// </summary>
        #region IsSelected
        public bool IsSelected
        {
            get => this._IsSelected;
            set => this.SetProperty(ref this._IsSelected, value);
        }
        private bool _IsSelected = default;
        #endregion
        #endregion

        /// <summary>
        /// 帳簿項目情報をクリアする
        /// </summary>
        public void ClearActionInfo()
        {
            this.ActionId = default;
            this.ItemName = default;
            this.ShopName = default;
            this.Remark = default;

            this.IsMatch = default;
        }
    }
}
