using HouseholdAccountBook.Models.ValueObjects;
using HouseholdAccountBook.ViewModels.Abstract;

namespace HouseholdAccountBook.Models.UiDto
{
    /// <summary>
    /// 関係性Model
    /// </summary>
    public class RelationModel : BindableBase
    {
        #region プロパティ
        /// <summary>
        /// 関係があるか
        /// </summary>
        public bool IsRelated { get; init; }

        /// <summary>
        /// 帳簿/項目ID
        /// </summary>
        public IdObj Id { get; init; }

        /// <summary>
        /// 表示名
        /// </summary>
        public string Name { get; init; }
        #endregion
    }
}
