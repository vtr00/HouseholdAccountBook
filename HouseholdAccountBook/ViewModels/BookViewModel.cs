namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// 帳簿VM
    /// </summary>
    public class BookViewModel
    {
        #region プロパティ
        /// <summary>
        /// 帳簿ID
        /// </summary>
        /// <note>null許容型の理由は？</note>
        public int? Id { get; set; }

        /// <summary>
        /// 帳簿名
        /// </summary>
        public string Name { get; set; }
        #endregion
    }
}
