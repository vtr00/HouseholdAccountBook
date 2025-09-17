using HouseholdAccountBook.Models.DbHandler;
using HouseholdAccountBook.Others;
using System;
using System.Windows;
using System.Windows.Input;

namespace HouseholdAccountBook.ViewModels.Abstract
{
    /// <summary>
    /// WindowViewModelの基底クラス
    /// </summary>
    public abstract class WindowViewModelBase : BindableBase
    {
        /// <summary>
        /// WaitCursorマネージャファクトリ
        /// </summary>
        protected WaitCursorManagerFactory waitCursorManagerFactory;
        /// <summary>
        /// DBハンドラファクトリ
        /// </summary>
        protected DbHandlerFactory dbHandlerFactory;

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
        /// ウィンドウの領域設定を指定する
        /// </summary>
        public abstract Rect WindowRectSetting { set; }
        /// <summary>
        /// ウィンドウのサイズ設定を取得する
        /// </summary>
        public abstract Size? WindowSizeSetting { get; }
        /// <summary>
        /// ウィンドウの位置設定を取得する
        /// </summary>
        public abstract Point? WindowPointSetting { get; }

        /// <summary>
        /// OKコマンド
        /// </summary>
        public ICommand OKCommand => new RelayCommand(this.OKCommand_Executed, this.OKCommand_CanExecute);
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
        /// キャンセルコマンド
        /// </summary>
        public ICommand CancelCommand => new RelayCommand(this.CanncelCommand_Executed);
        /// <summary>
        /// キャンセルコマンド処理
        /// </summary>
        protected virtual void CanncelCommand_Executed()
        {
            this.CloseRequest(new CloseRequestEventArgs(false));
        }

        /// <summary>
        /// クローズコマンド
        /// </summary>
        public ICommand CloseCommand => new RelayCommand(this.CloseCommand_Executed);
        /// <summary>
        /// クローズコマンド処理
        /// </summary>
        protected virtual void CloseCommand_Executed()
        {
            this.CloseRequest(new CloseRequestEventArgs(null));
        }

        /// <summary>
        /// 非表示コマンド
        /// </summary>
        public ICommand HideCommand => new RelayCommand(this.HideCommand_Executed);
        /// <summary>
        /// 非表示コマンド処理
        /// </summary>
        protected virtual void HideCommand_Executed()
        {
            this.HideRequest();
        }

        /// <summary>
        /// ファイル選択コマンド
        /// </summary>
        public virtual ICommand SelectFilePathCommand { get; set; } = null;

        /// <summary>
        /// ウィンドウのサイズ設定を取得する
        /// </summary>
        /// <param name="width">ウィンドウの幅設定</param>
        /// <param name="height">ウィンドウの高さ設定</param>
        /// <returns>ウィンドウのサイズ設定</returns>
        /// <remarks>設定が初期値であればNULLを返す</remarks>
        protected static Size? WindowSizeSettingImpl(double width, double height)
        {
            if (width != -1 && height != -1) {
                Size size = new() {
                    Width = width,
                    Height = height
                };
                return size;
            }
            else {
                return null;
            }
        }

        /// <summary>
        /// ウィンドウの位置設定を取得する
        /// </summary>
        /// <param name="left">ウィンドウの左位置設定</param>
        /// <param name="top">ウィンドウの上位置設定</param>
        /// <param name="isPositionSaved">ウィンドウ位置の保存有無設定</param>
        /// <returns>ウィンドウの位置設定</returns>
        /// <remarks>位置を保存しないまたは位置が不適切な場合はNULLを返す</remarks>
        protected static Point? WindowPointSettingImpl(double left, double top, bool isPositionSaved = true)
        {
            if (isPositionSaved && -10 <= left && 0 <= top) {
                Point point = new() {
                    X = left,
                    Y = top,
                };
                return point;
            }
            else {
                return null;
            }
        }

        /// <summary>
        /// ViewModelの初期化を行う
        /// </summary>
        /// <param name="waitCursorManagerFactory">WaitCursorマネージャファクトリ</param>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <remarks>コードビハインドのコンストラクタで呼び出す</remarks>
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
    }
}
