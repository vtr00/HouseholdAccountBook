using HouseholdAccountBook.Models.ValueObjects;
using System.Diagnostics;

namespace HouseholdAccountBook.Models.UiDto
{
    /// <summary>
    /// 項目Model
    /// </summary>
    /// <param name="id">項目ID</param>
    /// <param name="name">項目名</param>
    [DebuggerDisplay("ItemId: {ItemId} Name: {Name}")]
    public class ItemModel(ItemIdObj id, string name)
    {
        #region プロパティ
        /// <summary>
        /// 項目ID
        /// </summary>
        public ItemIdObj Id { get; init; } = id;

        /// <summary>
        /// ソート順
        /// </summary>
        public int SortOrder { get; init; } = -1;

        /// <summary>
        /// 項目名
        /// </summary>
        public string Name { get; init; } = name;

        /// <summary>
        /// 分類名
        /// </summary>
        public string CategoryName { private get; init; } = string.Empty;

        /// <summary>
        /// 分類併記名
        /// </summary>
        public string NameWithCategory => this.CategoryName != string.Empty ? $"{this.CategoryName} > {this.Name}" : this.Name;
        #endregion
    }
}
