using Prism.Mvvm;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// 関係性VM
    /// </summary>
    public class RelationViewModel : BindableBase
    {
        /// <summary>
        /// 関係があるか
        /// </summary>
        #region IsRelated
        public bool IsRelated
        {
            get { return _IsRelated; }
            set { SetProperty(ref _IsRelated, value); }
        }
        private bool _IsRelated = default(bool);
        #endregion

        /// <summary>
        /// ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 表示名
        /// </summary>
        public string Name { get; set; }
    }
}
