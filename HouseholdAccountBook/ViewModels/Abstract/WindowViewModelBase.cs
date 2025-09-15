using HouseholdAccountBook.Models.DbHandler;
using HouseholdAccountBook.Others;
using System;
using System.Windows;
using System.Windows.Input;

namespace HouseholdAccountBook.ViewModels.Abstract
{
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
        /// ウィンドウのクローズをリクエストする
        /// </summary>
        public event EventHandler<CloseRequestEventArgs> CloseRequested;
        /// <summary>
        /// ウィンドウの非表示をリクエストする
        /// </summary>
        public event EventHandler<EventArgs> HideRequested;
        /// <summary>
        /// ファイル選択ダイアログをリクエストする
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
        /// OKボタンクリック時のコマンド
        /// </summary>
        public ICommand OKCommand => new RelayCommand(this.OKCommand_Executed, this.OKCommand_CanExecute);
        /// <summary>
        /// OKボタンクリックの実行可否
        /// </summary>
        /// <returns></returns>
        protected virtual bool OKCommand_CanExecute() { return true; }
        /// <summary>
        /// OKボタンクリック時のコマンド処理
        /// </summary>
        protected virtual void OKCommand_Executed()
        {
            this.CloseRequest(new CloseRequestEventArgs(true));
        }

        /// <summary>
        /// キャンセルボタンクリック時のコマンド
        /// </summary>
        public ICommand CancelCommand => new RelayCommand(this.CanncelCommand_Executed);
        /// <summary>
        /// キャンセルボタンクリック時のコマンド処理
        /// </summary>
        protected virtual void CanncelCommand_Executed()
        {
            this.CloseRequest(new CloseRequestEventArgs(false));
        }

        public ICommand HideCommand => new RelayCommand(this.HideCommand_Executed);
        protected virtual void HideCommand_Executed()
        {
            this.HideRequest();
        }

        /// <summary>
        /// ファイル選択ボタンクリック時のコマンド
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
        /// ウィンドウのクローズをリクエストする
        /// </summary>
        /// <param name="e"></param>
        protected void CloseRequest(CloseRequestEventArgs e)
        {
            this.CloseRequested?.Invoke(this, e);
        }
        /// <summary>
        /// ウィンドウの非表示をリクエストする
        /// </summary>
        protected void HideRequest()
        {
            this.HideRequested?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// ファイル選択ダイアログをリクエストする
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
        /// ファイル選択ダイアログをリクエストする
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
