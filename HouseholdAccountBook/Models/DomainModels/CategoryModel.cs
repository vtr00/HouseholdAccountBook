namespace HouseholdAccountBook.Models.DomainModels
{
    /// <summary>
    /// 分類Model
    /// </summary>
    public class CategoryModel
    {
        #region プロパティ
        /// <summary>
        /// 分類ID
        /// </summary>
        public int Id { get; init; } = -1;
        /// <summary>
        /// 分類名
        /// </summary>
        public string Name { get; init; } = string.Empty;
        #endregion
    }
}
