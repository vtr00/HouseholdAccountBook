using HouseholdAccountBook.Models.ValueObjects;

namespace HouseholdAccountBook.Models.Args.RequestEventArgs
{
    /// <summary>
    /// CSVファイル比較リクエスト時のイベント引数
    /// </summary>
    public class CompareCsvFileRequestEventArgs : DbRequestEventArgsBase
    {
        /// <summary>
        /// 初期選択する帳簿のID
        /// </summary>
        public AccountIdObj InitialAccountId { get; set; }
    }
}
