using HouseholdAccountBook.ViewModels.Component;
using System;
using System.Collections.Generic;

namespace HouseholdAccountBook.Args.RequestEventArgs
{
    /// <summary>
    /// 帳簿項目追加要求イベント時のイベント引数
    /// </summary>
    public class AddActionRequestEventArgs : DbRequestEventArgsBase
    {
        /// <summary>
        /// 初期選択する帳簿のID
        /// </summary>
        public int? InitialBookId { get; set; }
        /// <summary>
        /// 初期選択する月
        /// </summary>
        public DateTime? InitialMonth { get; set; }
        /// <summary>
        /// 初期選択する日付
        /// </summary>
        public DateTime? InitialDate { get; set; }
        /// <summary>
        /// 初期表示するCSVレコード
        /// </summary>
        public CsvViewModel InitialRecord { get; set; }

        /// <summary>
        /// 登録完了時イベントハンドラ
        /// </summary>
        public EventHandler<EventArgs<List<int>>> Registered { get; set; }
    }
}
