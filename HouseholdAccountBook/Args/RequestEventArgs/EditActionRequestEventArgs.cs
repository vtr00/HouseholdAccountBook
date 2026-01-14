using System;
using System.Collections.Generic;

namespace HouseholdAccountBook.Args.RequestEventArgs
{
    /// <summary>
    /// 帳簿項目編集要求イベント時のイベント引数
    /// </summary>
    public class EditActionRequestEventArgs : DbRequestEventArgsBase
    {
        /// <summary>
        /// 編集対象の帳簿項目のID
        /// </summary>
        public int TargetActionId { get; set; }

        /// <summary>
        /// 登録完了時イベントハンドラ
        /// </summary>
        public EventHandler<EventArgs<List<int>>> Registered { get; set; }
    }
}
