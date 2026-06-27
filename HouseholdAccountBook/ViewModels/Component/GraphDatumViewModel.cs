using HouseholdAccountBook.Models.UiDto;
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
        /// 値(主単位/Y軸用)
        /// </summary>
        public decimal Value { get; init; }

        /// <summary>
        /// 日付(Tracker表示用)
        /// </summary>
        public DateOnly Date { get; init; }
        /// <summary>
        /// 値(Tracker表示用)
        /// </summary>
        public string DisplayValue { get; init; }

        /// <summary>
        /// インデックス(LineSeriesのX軸用)
        /// </summary>
        public int Index { get; init; }

        /// <summary>
        /// 分類
        /// </summary>
        public CategoryModel Category { get; init; }

        /// <summary>
        /// 項目
        /// </summary>
        public ItemModel Item { get; init; }
        #endregion
    }
}
