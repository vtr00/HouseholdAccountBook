using HouseholdAccountBook.Infrastructure.CSV;
using HouseholdAccountBook.Models.ValueObjects;
using System;
using System.Collections.Generic;

namespace HouseholdAccountBook.Infrastructure.Utilities.Args.RequestEventArgs
{
    /// <summary>
    /// 帳簿項目リスト追加要求イベント時のイベント引数
    /// </summary>
    public class AddActionListRequestEventArgs : DbRequestEventArgsBase
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
        /// 初期表示するCSVレコードリスト
        /// </summary>
        public IEnumerable<ActionCsvDto> InitialRecordList { get; set; }

        /// <summary>
        /// 登録完了時イベントハンドラ
        /// </summary>
        public EventHandler<EventArgs<IEnumerable<ActionIdObj>>> Registered { get; set; }
    }
}
