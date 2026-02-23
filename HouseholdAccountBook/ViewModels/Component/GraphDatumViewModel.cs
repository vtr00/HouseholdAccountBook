using System;

namespace HouseholdAccountBook.ViewModels.Component
{
    /// <summary>
    /// グラフデータVM
    /// </summary>
    public class GraphDatumViewModel
    {
        #region プロパティ
        /// <summary>
        /// 値(Y軸用)
        /// </summary>
        #region Value
        public int Value { get; set; }
        #endregion

        /// <summary>
        /// 日付(Tracker用)
        /// </summary>
        #region Date
        public DateTime Date { get; set; }
        #endregion

        /// <summary>
        /// インデックス(LineSeriesのX軸用)
        /// </summary>
        #region Index
        public int Index { get; set; }
        #endregion

        /// <summary>
        /// 項目ID
        /// </summary>
        #region ItemId
        public int ItemId { get; set; }
        #endregion

        /// <summary>
        /// 分類ID
        /// </summary>
        #region CategoryId
        public int CategoryId { get; set; }
        #endregion
        #endregion
    }
}
