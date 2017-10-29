using Prism.Mvvm;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// グラフ項目VM
    /// </summary>
    public class SeriesItemViewModel : BindableBase
    {
        #region プロパティ
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
        /// 番号(月 or 日)
        /// </summary>
        #region Number
        public int Number
        {
            get { return this._Number; }
            set { SetProperty(ref this._Number, value); }
        }
        private int _Number = default(int);
        #endregion

        /// <summary>
        /// 項目ID
        /// </summary>
        #region ItemId
        public int ItemId
        {
            get { return this._ItemId; }
            set { SetProperty(ref this._ItemId, value); }
        }
        private int _ItemId = default(int);
        #endregion

        /// <summary>
        /// 分類ID
        /// </summary>
        #region CategoryId
        public int CategoryId
        {
            get { return this._CategoryId; }
            set { SetProperty(ref this._CategoryId, value); }
        }
        private int _CategoryId = default(int);
        #endregion
        #endregion
    }
}
