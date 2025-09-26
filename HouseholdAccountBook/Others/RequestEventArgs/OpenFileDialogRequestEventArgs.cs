using System;

namespace HouseholdAccountBook.Others.RequestEventArgs
{
    public class OpenFileDialogRequestEventArgs : EventArgs
    {
        /// <summary>
        /// ファイルが存在するか
        /// </summary>
        public bool CheckFileExists { get; set; } = true;
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
        /// 複数選択可能にするか
        /// </summary>
        public bool Multiselect { get; set; } = false;
        /// <summary>
        /// 結果ファイル(複数)
        /// </summary>
        public string[] FileNames { get; set; }
        /// <summary>
        /// ダイアログの結果
        /// </summary>
        public bool Result { get; set; } = false;
        /// <summary>
        /// ファイルが存在するフォルダが存在するか
        /// </summary>
        public bool CheckPathExists { get; set; } = true;
    }
}
