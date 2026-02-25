using HouseholdAccountBook.Models;

namespace HouseholdAccountBook.Models.DomainModels
{
    /// <summary>
    /// 帳簿Model
    /// </summary>
    public class BookModel
    {
        #region プロパティ
        /// <summary>
        /// 帳簿ID
        /// </summary>
        public int? Id { get; init; }

        /// <summary>
        /// 帳簿名
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// 備考
        /// </summary>
        public string Remark { get; init; }

        /// <summary>
        /// 帳簿種別
        /// </summary>
        public BookKind BookKind { get; init; }

        /// <summary>
        /// 支払日
        /// </summary>
        public int? PayDay { get; init; }

        /// <summary>
        /// 支払い元帳簿ID
        /// </summary>
        public int? DebitBookId { get; init; }
        #endregion
    }
}
