namespace HouseholdAccountBook.ViewModels
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
        public bool IsRelated
        {
            get => this._IsRelated;
            set => this.SetProperty(ref this._IsRelated, value);
        }
        private bool _IsRelated = default;
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
