using HouseholdAccountBook.ViewModels.Abstract;
using System;

namespace HouseholdAccountBook.ViewModels.Component
{
    /// <summary>
    /// グラフデータVM
    /// </summary>
    public class GraphDatumViewModel : BindableBase
    {
        #region プロパティ
        /// <summary>
        /// 値(Y軸用)
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
        /// 日付(Tracker用)
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
        /// インデックス(LineSeriesのX軸用)
        /// </summary>
        #region Index
        public int Index
        {
            get => this._Index;
            set => this.SetProperty(ref this._Index, value);
        }
        private int _Index = default;
        #endregion

        /// <summary>
        /// 項目ID
        /// </summary>
        #region ItemId
        public int ItemId
        {
            get => this._ItemId;
            set => this.SetProperty(ref this._ItemId, value);
        }
        private int _ItemId = default;
        #endregion

        /// <summary>
        /// 分類ID
        /// </summary>
        #region CategoryId
        public int CategoryId
        {
            get => this._CategoryId;
            set => this.SetProperty(ref this._CategoryId, value);
        }
        private int _CategoryId = default;
        #endregion
        #endregion
    }
}
