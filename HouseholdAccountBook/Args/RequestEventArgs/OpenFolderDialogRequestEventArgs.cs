using System;

namespace HouseholdAccountBook.Args.RequestEventArgs
{
    /// <summary>
    /// フォルダ選択ダイアログ要求時イベントのイベント引数
    /// </summary>
    public class OpenFolderDialogRequestEventArgs : EventArgs
    {
        /// <summary>
        /// 初期ディレクトリ
        /// </summary>
        public string InitialDirectory { get; set; }
        /// <summary>
        /// 初期フォルダ名及び結果フォルダ名
        /// </summary>
        public string FolderName { get; set; }
        /// <summary>
        /// ダイアログタイトル
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// ダイアログの結果
        /// </summary>
        public bool Result { get; set; }
    }
}
