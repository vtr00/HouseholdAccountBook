namespace HouseholdAccountBook.Models.DomainModels
{
    /// <summary>
    /// 項目Model
    /// </summary>
    public class ItemModel
    {
        #region プロパティ
        /// <summary>
        /// 項目ID
        /// </summary>
        public int Id { get; init; } = -1;

        /// <summary>
        /// 項目名
        /// </summary>
        public string Name { get; init; } = string.Empty;

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
