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
        /// 月
        /// </summary>
        #region Month
        public int Month
        {
            get { return _Month; }
            set { SetProperty(ref _Month, value); }
        }
        private int _Month = default(int);
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
