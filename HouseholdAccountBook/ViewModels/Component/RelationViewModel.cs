using HouseholdAccountBook.ViewModels.Abstract;

namespace HouseholdAccountBook.ViewModels.Component
{
    /// <summary>
    /// 関係性VM
    /// </summary>
    public class RelationViewModel : BindableBase
    {
        #region プロパティ
        /// <summary>
        /// 関係があるか
        /// </summary>
        #region IsRelated
        public bool IsRelated {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 表示名
        /// </summary>
        public string Name { get; set; }
        #endregion
    }
}
