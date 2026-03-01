using HouseholdAccountBook.Infrastructure.Utilities.Args;
using HouseholdAccountBook.Models.ValueObjects;
using System;
using System.Collections.Generic;

namespace HouseholdAccountBook.Infrastructure.Utilities.Args.RequestEventArgs
{
    /// <summary>
    /// 移動編集要求イベント時のイベント引数
    /// </summary>
    public class EditMoveRequestEventArgs : DbRequestEventArgsBase
    {
        /// <summary>
        /// 編集対象の帳簿項目のグループID
        /// </summary>
        public GroupIdObj TargetGroupId { get; set; }

        /// <summary>
        /// 登録完了時イベントハンドラ
        /// </summary>
        public EventHandler<EventArgs<List<ActionIdObj>>> Registered { get; set; }
    }
}
