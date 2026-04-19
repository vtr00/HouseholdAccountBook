using HouseholdAccountBook.Infrastructure.DB.DbHandlers;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.Utilities.Args.RequestEventArgs;
using HouseholdAccountBook.Models.AppServices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HouseholdAccountBook.ViewModels.Abstract
{
    /// <summary>
    /// ウィンドウ内の一部を構成するViewModelの基底クラス
    /// </summary>
    public abstract class WindowPartViewModelBase : BindableBase
    {
        #region フィールド
        /// <summary>
        /// 子ViewModelリスト
        /// </summary>
        protected readonly List<WindowPartViewModelBase> mChildrenVM = [];

        /// <summary>
        /// 処理中状態サービス
        /// </summary>
        protected BusyService mBusyService;
        /// <summary>
        /// DBハンドラファクトリ
        /// </summary>
        protected DbHandlerFactory mDbHandlerFactory;
        #endregion

        #region イベント
        /// <summary>
        /// ウィンドウクローズ要求イベント
        /// </summary>
        public event EventHandler<DialogCloseRequestEventArgs> CloseRequested;
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
        public ICommand OKCommand => field ??= new RelayCommand(this.OKCommand_Execute, this.OKCommand_CanExecute);
        /// <summary>
        /// OKコマンド実行可能か
        /// </summary>
        /// <returns></returns>
        protected virtual bool OKCommand_CanExecute() => true;
        /// <summary>
        /// OKコマンド処理
        /// </summary>
        protected virtual void OKCommand_Execute() => this.CloseRequest(new DialogCloseRequestEventArgs(true));

        /// <summary>
        /// キャンセルコマンド
        /// </summary>
        public ICommand CancelCommand => field ??= new RelayCommand(this.CanncelCommand_Execute);
        /// <summary>
        /// キャンセルコマンド処理
        /// </summary>
        protected virtual void CanncelCommand_Execute() => this.CloseRequest(new DialogCloseRequestEventArgs(false));

        /// <summary>
        /// クローズコマンド
        /// </summary>
        public ICommand CloseCommand => field ??= new RelayCommand(this.CloseCommand_Execute);
        /// <summary>
        /// クローズコマンド処理
        /// </summary>
        protected virtual void CloseCommand_Execute() => this.CloseRequest(new DialogCloseRequestEventArgs(null));

        /// <summary>
        /// 非表示コマンド
        /// </summary>
        public ICommand HideCommand => field ??= new RelayCommand(this.HideCommand_Execute);
        /// <summary>
        /// 非表示コマンド処理
        /// </summary>
        protected virtual void HideCommand_Execute() => this.HideRequest();
        #endregion

        /// <summary>
        /// 初期化済か
        /// </summary>
        public bool Initialized { get; protected set; }

        /// <summary>
        /// ViewModelの初期化を行う
        /// </summary>
        /// <param name="busyService">処理中状態サービス</param>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <remarks><see cref="FrameworkElement">のコンストラクタで呼び出す</remarks>
        public virtual void Initialize(BusyService busyService, DbHandlerFactory dbHandlerFactory)
        {
            using FuncLog funcLog = new();

            this.mBusyService = busyService;
            this.mDbHandlerFactory = dbHandlerFactory;

            foreach (WindowPartViewModelBase childVM in this.mChildrenVM) {
                childVM.Initialize(this.mBusyService, this.mDbHandlerFactory);
            }

            this.Initialized = true;
        }

        /// <summary>
        /// 表示する情報を読み込む
        /// </summary>
        /// <remarks><see cref="FrameworkElement.Loaded">内で呼び出す. 初期表示値はここで設定する</remarks>
        public abstract Task LoadAsync();

        /// <summary>
        /// イベントハンドラを登録する
        /// </summary>
        /// <remarks><see cref="FrameworkElement.Loaded">内で<see cref="LoadAsync"/> のあとで呼び出す</remarks>
        public abstract void AddEventHandlers();

        /// <summary>
        /// 表示する情報を保存する
        /// </summary>
        protected virtual Task SaveAsync() => throw new NotImplementedException();

        /// <summary>
        /// ウィンドウクローズ要求を発行する
        /// </summary>
        /// <param name="e"></param>
        protected void CloseRequest(DialogCloseRequestEventArgs e)
        {
            using FuncLog funcLog = new(new { e });

            this.CloseRequested?.Invoke(this, e);
        }

        /// <summary>
        /// ウィンドウ非表示要求を発行する
        /// </summary>
        protected void HideRequest()
        {
            using FuncLog funcLog = new();

            this.HideRequested?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// ファイル選択ダイアログ要求を発行する
        /// </summary>
        /// <param name="e"></param>
        protected bool OpenFileDialogRequest(OpenFileDialogRequestEventArgs e)
        {
            using FuncLog funcLog = new(new { e });

            e.Multiselect = false;
            this.OpenFileDialogRequested?.Invoke(this, e);
            return e.Result;
        }

        /// <summary>
        /// ファイル選択ダイアログ要求を発行する(複数選択版)
        /// </summary>
        /// <param name="e"></param>
        protected bool OpenFilesDialogRequest(OpenFileDialogRequestEventArgs e)
        {
            using FuncLog funcLog = new(new { e });

            e.Multiselect = true;
            this.OpenFileDialogRequested?.Invoke(this, e);
            return e.Result;
        }

        /// <summary>
        /// フォルダ選択ダイアログ要求を発行する
        /// </summary>
        /// <param name="e"></param>
        protected bool OpenFolderDialogRequest(OpenFolderDialogRequestEventArgs e)
        {
            using FuncLog funcLog = new(new { e });

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
            using FuncLog funcLog = new(new { e });

            this.SaveFileDialogRequested?.Invoke(this, e);
            return e.Result;
        }
    }
}
