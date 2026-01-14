using HouseholdAccountBook.Args;
using HouseholdAccountBook.ViewModels.Abstract;
using System;

namespace HouseholdAccountBook.ViewModels.Component
{
    /// <summary>
    /// CSV比較VM
    /// </summary>
    public partial class CsvComparisonViewModel : BindableBase, ISelectable
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
        public int? ActionId {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 項目名
        /// </summary>
        #region ItemName
        public string ItemName {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 店舗名
        /// </summary>
        #region ShopName
        public string ShopName {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 備考
        /// </summary>
        #region Remark
        public string Remark {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// グループID
        /// </summary>
        #region GroupId
        public int? GroupId {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 一致フラグ
        /// </summary>
        #region IsMatch
        public bool IsMatch {
            get;
            set {
                if (this.ActionId.HasValue) {
                    if (this.SetProperty(ref field, value)) {
                        this.IsMatchChanged?.Invoke(new EventArgs<int?, bool>(this.ActionId, value));
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// CSVデータ
        /// </summary>
        #region Record
        public CsvViewModel Record {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 選択フラグ
        /// </summary>
        #region IsSelected
        public bool SelectFlag {
            get;
            set => this.SetProperty(ref field, value);
        }
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
