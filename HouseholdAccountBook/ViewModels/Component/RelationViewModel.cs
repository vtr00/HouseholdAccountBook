using HouseholdAccountBook.Models.UiDto;
using HouseholdAccountBook.Models.ValueObjects;
using HouseholdAccountBook.ViewModels.Abstract;

namespace HouseholdAccountBook.ViewModels.Component
{
    /// <summary>
    /// 関係性VM
    /// </summary>
    public class RelationViewModel(RelationModel relation) : BindableBase
    {
        /// <summary>
        /// 関係性Model
        /// </summary>
        public RelationModel Relation { get; set; } = relation;

        /// <summary>
        /// 関係があるか
        /// </summary>
        #region IsRelated
        public bool IsRelated {
            get;
            set => this.SetProperty(ref field, value);
        } = relation.IsRelated;
        #endregion

        /// <summary>
        /// 帳簿/項目ID
        /// </summary>
        public IdObj Id => this.Relation.Id;

        /// <summary>
        /// 表示名
        /// </summary>
        public string Name => this.Relation.Name;
    }
}
