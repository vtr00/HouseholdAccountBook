namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// 項目VM
    /// </summary>
    public partial class ItemViewModel
    {
        #region プロパティ
        /// <summary>
        /// 項目ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 項目名
        /// </summary>
        public string Name { private get; set; }

        /// <summary>
        /// 分類名
        /// </summary>
        public string CategoryName { private get; set; }

        /// <summary>
        /// 分類併記名
        /// </summary>
        public string NameWithCategory => this.CategoryName != "" ? this.CategoryName + " > " + this.Name : this.Name;
        #endregion
    }
}
