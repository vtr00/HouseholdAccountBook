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
        /// <note>TODO: null許容型の理由は？</note>
        public int? Id { get; set; }

        /// <summary>
        /// 帳簿名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 備考
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 帳簿種別
        /// </summary>
        public BookKind BookKind { get; set; }

        /// <summary>
        /// 支払日
        /// </summary>
        public int? PayDay { get; set; }

        /// <summary>
        /// 支払い元帳簿ID
        /// </summary>
        public int? DebitBookId { get; set; }
        #endregion
    }
}
