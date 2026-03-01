using HouseholdAccountBook.Infrastructure.CSV;
using HouseholdAccountBook.Infrastructure.Utilities.Args;
using HouseholdAccountBook.Models.ValueObjects;
using HouseholdAccountBook.ViewModels.Component;
using System;
using System.Collections.Generic;

namespace HouseholdAccountBook.Infrastructure.Utilities.Args.RequestEventArgs
{
    /// <summary>
    /// 帳簿項目追加要求イベント時のイベント引数
    /// </summary>
    public class AddActionRequestEventArgs : DbRequestEventArgsBase
    {
        /// <summary>
        /// 初期選択する帳簿のID
        /// </summary>
        public BookIdObj InitialBookId { get; set; }
        /// <summary>
        /// 初期選択する月
        /// </summary>
        public DateOnly? InitialMonth { get; set; }
        /// <summary>
        /// 初期選択する日付
        /// </summary>
        public DateOnly? InitialDate { get; set; }
        /// <summary>
        /// 初期表示するCSVレコード
        /// </summary>
        public ActionCsvDto InitialRecord { get; set; }

        /// <summary>
        /// 登録完了時イベントハンドラ
        /// </summary>
        public EventHandler<EventArgs<List<ActionIdObj>>> Registered { get; set; }
    }
}
