using System;
using System.Collections.Generic;

namespace HouseholdAccountBook.Others.RequestEventArgs
{
    /// <summary>
    /// 移動複製要求イベント時のイベント引数
    /// </summary>
    public class CopyMoveRequestEventArgs : DbRequestEventArgsBase
    {
        /// <summary>
        /// 複製対象の帳簿項目のグループID
        /// </summary>
        public int TargetGroupId { get; set; }

        /// <summary>
        /// 登録完了時イベントハンドラ
        /// </summary>
        public EventHandler<EventArgs<List<int>>> Registered { get; set; }
    }
}
