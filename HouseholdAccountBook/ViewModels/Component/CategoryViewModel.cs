using HouseholdAccountBook.ViewModels.Abstract;

namespace HouseholdAccountBook.ViewModels.Component
{
    /// <summary>
    /// 分類VM
    /// </summary>
    public class CategoryViewModel : BindableBase
    {
        #region プロパティ
        /// <summary>
        /// 分類ID
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 分類名
        /// </summary>
        public string Name { get; set; }
        #endregion
    }
}
