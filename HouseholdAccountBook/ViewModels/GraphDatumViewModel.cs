using Prism.Mvvm;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// グラフデータVM
    /// </summary>
    public class GraphDatumViewModel : BindableBase
    {
        #region プロパティ
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
        /// 番号(月 or 日)
        /// </summary>
        #region Number
        public int Number
        {
            get => this._Number;
            set => this.SetProperty(ref this._Number, value);
        }
        private int _Number = default;
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
