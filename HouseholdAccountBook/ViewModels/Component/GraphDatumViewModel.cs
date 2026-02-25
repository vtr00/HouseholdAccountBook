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
        public int Value { get; init; }

        /// <summary>
        /// 日付(Tracker用)
        /// </summary>
        public DateTime Date { get; init; }

        /// <summary>
        /// インデックス(LineSeriesのX軸用)
        /// </summary>
        public int Index { get; init; }

        /// <summary>
        /// 項目ID
        /// </summary>
        public int ItemId { get; init; }

        /// <summary>
        /// 分類ID
        /// </summary>
        public int CategoryId { get; init; }
        #endregion
    }
}
