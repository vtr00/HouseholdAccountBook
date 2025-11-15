namespace HouseholdAccountBook.Others.RequestEventArgs
{
    /// <summary>
    /// 設定編集要求イベント時のイベント引数
    /// </summary>
    public class SettingsRequestEventArgs : DbRequestEventArgsBase
    {
        /// <summary>
        /// 設定変更結果
        /// </summary>
        public bool Result { get; set; }
    }
}
