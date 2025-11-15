using System;

namespace HouseholdAccountBook.Others.RequestEventArgs
{
    /// <summary>
    /// ダイアログを閉じる要求イベントのイベント引数
    /// </summary>
    /// <param name="result">ダイアログの結果</param>
    public class DialogCloseRequestEventArgs(bool? result) : EventArgs
    {
        /// <summary>
        /// ダイアログの結果
        /// </summary>
        public bool? Result { get; set; } = result;
    }
}
