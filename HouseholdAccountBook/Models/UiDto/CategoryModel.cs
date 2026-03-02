using HouseholdAccountBook.Models.ValueObjects;

namespace HouseholdAccountBook.Models.UiDto
{
    /// <summary>
    /// 分類Model
    /// </summary>
    /// <param name="id">分類ID</param>
    /// <param name="name">分類名</param>
    /// <param name="kind">収支種別</param>
    public class CategoryModel(CategoryIdObj id, string name, BalanceKind kind)
    {
        #region プロパティ
        /// <summary>
        /// 分類ID
        /// </summary>
        public CategoryIdObj Id { get; init; } = id;

        /// <summary>
        /// ソート順
        /// </summary>
        public int SortOrder { get; init; } = -1;

        /// <summary>
        /// 分類名
        /// </summary>
        public string Name { get; init; } = name;

        /// <summary>
        /// 収支種別
        /// </summary>
        public BalanceKind BalanceKind { get; init; } = kind;
        #endregion
    }
}
