using HouseholdAccountBook.Adapters.DbHandler;
using HouseholdAccountBook.Others;
using HouseholdAccountBook.Others.RequestEventArgs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HouseholdAccountBook.ViewModels.Abstract
{
    /// <summary>
    /// ウィンドウ内の一部を構成するViewModelの基底クラス
    /// </summary>
    public abstract class WindowPartViewModelBase : BindableBase
    {
        protected readonly List<WindowPartViewModelBase> childrenVM = [];

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
        /// <summary>
        /// ファイル保存ダイアログ要求イベント
        /// </summary>
        public event EventHandler<SaveFileDialogRequestEventArgs> SaveFileDialogRequested;
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

        #region コマンドイベントハンドラ
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
        #endregion

        /// <summary>
        /// ViewModelの初期化を行う
        /// </summary>
        /// <param name="waitCursorManagerFactory">WaitCursorマネージャファクトリ</param>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <remarks>コードビハインドのコンストラクタで呼び出す</remarks>
        public void Initialize(WaitCursorManagerFactory waitCursorManagerFactory, DbHandlerFactory dbHandlerFactory)
        {
            this.waitCursorManagerFactory = waitCursorManagerFactory;
            this.dbHandlerFactory = dbHandlerFactory;

            foreach (WindowPartViewModelBase childVM in this.childrenVM) {
                childVM.Initialize(waitCursorManagerFactory, dbHandlerFactory);
            }

            this.AddEventHandlers();
        }

        /// <summary>
        /// イベントハンドラをWVMに登録する
        /// </summary>
        /// <remarks><see cref="Initialize(WaitCursorManagerFactory, DbHandlerFactory)"/> で呼び出す</remarks>
        protected abstract void AddEventHandlers();

        /// <summary>
        /// 表示する情報を読み込む
        /// </summary>
        public abstract Task LoadAsync();

        /// <summary>
        /// 表示する情報を保存する
        /// </summary>
        protected virtual Task SaveAsync()
        {
            throw new NotImplementedException();
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
        protected bool OpenFileDialogRequest(OpenFileDialogRequestEventArgs e)
        {
            e.Multiselect = false;
            this.OpenFileDialogRequested?.Invoke(this, e);
            return e.Result;
        }

        /// <summary>
        /// ファイル選択ダイアログ要求を発行する(複数選択版)
        /// </summary>
        /// <param name="e"></param>
        /// <param name="filesSelected"></param>
        protected bool OpenFilesDialogRequest(OpenFileDialogRequestEventArgs e)
        {
            e.Multiselect = true;
            this.OpenFileDialogRequested?.Invoke(this, e);
            return e.Result;
        }

        /// <summary>
        /// フォルダ選択ダイアログ要求を発行する
        /// </summary>
        /// <param name="e"></param>
        /// <param name="folderSelected"></param>
        protected bool OpenFolderDialogRequest(OpenFolderDialogRequestEventArgs e)
        {
            this.OpenFolderDialogRequested?.Invoke(this, e);
            return e.Result;
        }

        /// <summary>
        /// ファイル保存ダイアログ要求を発行する
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        protected bool SaveFileDialogRequest(SaveFileDialogRequestEventArgs e)
        {
            this.SaveFileDialogRequested?.Invoke(this, e);
            return e.Result;
        }
    }
}
