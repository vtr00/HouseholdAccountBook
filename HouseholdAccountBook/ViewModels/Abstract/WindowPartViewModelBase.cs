using HouseholdAccountBook.Models.DbHandler;
using HouseholdAccountBook.Others;
using System;
using System.Windows.Input;

namespace HouseholdAccountBook.ViewModels.Abstract
{
    public class WindowPartViewModelBase : BindableBase
    {
        #region フィールド
        /// <summary>
        /// WaitCursorマネージャファクトリ
        /// </summary>
        protected WaitCursorManagerFactory waitCursorManagerFactory;
        /// <summary>
        /// DBハンドラファクトリ
        /// </summary>
        protected DbHandlerFactory dbHandlerFactory;
        #endregion

        #region イベント
        /// <summary>
        /// ウィンドウクローズ要求イベント
        /// </summary>
        public event EventHandler<CloseRequestEventArgs> CloseRequested;
        /// <summary>
        /// ウィンドウ非表示要求イベント
        /// </summary>
        public event EventHandler HideRequested;
        /// <summary>
        /// ファイル選択ダイアログ要求イベント
        /// </summary>
        public event EventHandler<OpenFileDialogRequestEventArgs> OpenFileDialogRequested;
        /// <summary>
        /// フォルダ選択ダイアログ要求イベント
        /// </summary>
        public event EventHandler<OpenFolderDialogRequestEventArgs> OpenFolderDialogRequested;
        #endregion

        #region Bindingプロパティ
        /// <summary>
        /// OKコマンド
        /// </summary>
        public ICommand OKCommand => new RelayCommand(this.OKCommand_Executed, this.OKCommand_CanExecute);
        /// <summary>
        /// キャンセルコマンド
        /// </summary>
        public ICommand CancelCommand => new RelayCommand(this.CanncelCommand_Executed);
        /// <summary>
        /// クローズコマンド
        /// </summary>
        public ICommand CloseCommand => new RelayCommand(this.CloseCommand_Executed);
        /// <summary>
        /// 非表示コマンド
        /// </summary>
        public ICommand HideCommand => new RelayCommand(this.HideCommand_Executed);
        /// <summary>
        /// ファイル選択コマンド
        /// </summary>
        public virtual ICommand SelectFilePathCommand { get; set; } = null;
        /// <summary>
        /// フォルダ選択コマンド
        /// </summary>
        public virtual ICommand SelectFolderPathCommand { get; set; } = null;
        #endregion

        /// <summary>
        /// OKコマンド実行可能か
        /// </summary>
        /// <returns></returns>
        protected virtual bool OKCommand_CanExecute() { return true; }
        /// <summary>
        /// OKコマンド処理
        /// </summary>
        protected virtual void OKCommand_Executed()
        {
            this.CloseRequest(new CloseRequestEventArgs(true));
        }

        /// <summary>
        /// キャンセルコマンド処理
        /// </summary>
        protected virtual void CanncelCommand_Executed()
        {
            this.CloseRequest(new CloseRequestEventArgs(false));
        }

        /// <summary>
        /// クローズコマンド処理
        /// </summary>
        protected virtual void CloseCommand_Executed()
        {
            this.CloseRequest(new CloseRequestEventArgs(null));
        }

        /// <summary>
        /// 非表示コマンド処理
        /// </summary>
        protected virtual void HideCommand_Executed()
        {
            this.HideRequest();
        }

        /// <summary>
        /// ViewModelの初期化を行う
        /// </summary>
        /// <param name="waitCursorManagerFactory">WaitCursorマネージャファクトリ</param>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <remarks>コードビハインドのコンストラクタで最初に呼び出す</remarks>
        public virtual void Initialize(WaitCursorManagerFactory waitCursorManagerFactory, DbHandlerFactory dbHandlerFactory)
        {
            this.waitCursorManagerFactory = waitCursorManagerFactory;
            this.dbHandlerFactory = dbHandlerFactory;
        }

        /// <summary>
        /// ウィンドウクローズ要求を発行する
        /// </summary>
        /// <param name="e"></param>
        protected void CloseRequest(CloseRequestEventArgs e)
        {
            this.CloseRequested?.Invoke(this, e);
        }
        /// <summary>
        /// ウィンドウ非表示要求を発行する
        /// </summary>
        protected void HideRequest()
        {
            this.HideRequested?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// ファイル選択ダイアログ要求を発行する
        /// </summary>
        /// <param name="e"></param>
        /// <param name="fileSelected"></param>
        protected void OpenFileDialogRequest(OpenFileDialogRequestEventArgs e, Action<string> fileSelected)
        {
            e.Multiselect = false;
            this.OpenFileDialogRequested?.Invoke(this, e);
            if (e.Result == true) {
                fileSelected?.Invoke(e.FileName);
            }
        }

        /// <summary>
        /// ファイル選択ダイアログ要求を発行する(複数選択版)
        /// </summary>
        /// <param name="e"></param>
        /// <param name="filesSelected"></param>
        protected void OpenFilesDialogRequest(OpenFileDialogRequestEventArgs e, Action<string[]> filesSelected)
        {
            e.Multiselect = true;
            this.OpenFileDialogRequested?.Invoke(this, e);
            if (e.Result == true) {
                filesSelected?.Invoke(e.FileNames);
            }
        }

        /// <summary>
        /// フォルダ選択ダイアログ要求を発行する
        /// </summary>
        /// <param name="e"></param>
        /// <param name="folderSelected"></param>
        protected void OpenFolderDialogRequest(OpenFolderDialogRequestEventArgs e, Action<string> folderSelected)
        {
            this.OpenFolderDialogRequested?.Invoke(this, e);
            if (e.Result == true) {
                folderSelected?.Invoke(e.FolderName);
            }
        }
    }
}
