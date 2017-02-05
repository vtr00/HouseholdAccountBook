using System;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// 帳簿VM
    /// </summary>
    public class BookViewModel
    {
        /// <summary>
        /// 帳簿ID
        /// </summary>
        public int? BookId { get; set; }
        /// <summary>
        /// 帳簿名
        /// </summary>
        public String BookName { get; set; }
    }
}
