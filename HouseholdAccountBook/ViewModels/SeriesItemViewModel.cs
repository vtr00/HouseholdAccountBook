using Prism.Mvvm;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// グラフ項目VM
    /// </summary>
    public class SeriesItemViewModel : BindableBase
    {
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
        /// 番号(月/日)
        /// </summary>
        #region Number
        public int Number
        {
            get { return _Number; }
            set { SetProperty(ref _Number, value); }
        }
        private int _Number = default(int);
        #endregion

        /// <summary>
        /// 項目ID
        /// </summary>
        #region ItemId
        public int ItemId
        {
            get { return _ItemId; }
            set { SetProperty(ref _ItemId, value); }
        }
        private int _ItemId = default(int);
        #endregion

        /// <summary>
        /// 分類ID
        /// </summary>
        #region CategoryId
        public int CategoryId
        {
            get { return _CategoryId; }
            set { SetProperty(ref _CategoryId, value); }
        }
        private int _CategoryId = default(int);
        #endregion
    }
}
