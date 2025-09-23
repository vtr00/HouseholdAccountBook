namespace HouseholdAccountBook.Others
{
    public class SaveFileDialogRequestEventArgs
    {
        /// <summary>
        /// 初期ディレクトリ
        /// </summary>
        public string InitialDirectory { get; set; }
        /// <summary>
        /// 初期ファイル及び結果ファイル
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// ダイアログのタイトル
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// フィルタ
        /// </summary>
        public string Filter { get; set; }
        /// <summary>
        /// ダイアログの結果
        /// </summary>
        public bool Result { get; set; } = false;
    }
}
