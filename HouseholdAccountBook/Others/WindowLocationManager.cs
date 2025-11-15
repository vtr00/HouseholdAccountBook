using HouseholdAccountBook.Adapters.Logger;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.Views.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace HouseholdAccountBook.Others
{
    /// <summary>
    /// ウィンドウの位置を管理する
    /// </summary>
    public class WindowLocationManager : SingletonBase<WindowLocationManager>
    {
        /// <summary>
        /// ウィンドウログ
        /// </summary>
        private readonly Dictionary<Window, WindowLog> mLogDic = [];
        /// <summary>
        /// 初期値
        /// </summary>
        private readonly Dictionary<Window, Rect> mInitialRectDic = [];
        /// <summary>
        /// 最終補正値
        /// </summary>
        private readonly Dictionary<Window, Rect> mLastRectDic = [];

        /// <summary>
        /// スタティックコンストラクタ
        /// </summary>
        static WindowLocationManager() => Register(static () => new WindowLocationManager());
        /// <summary>
        /// プライベートコンストラクタ
        /// </summary>
        private WindowLocationManager() { }

        /// <summary>
        /// ウィンドウに位置監視機能を追加する
        /// </summary>
        /// <param name="window">対象のウィンドウ</param>
        public void Add(Window window)
        {
            if (window.Name == string.Empty) {
                window.Name = window.Title;
            }

            if (this.mLogDic.ContainsKey(window)) { return; }

            WindowLog log = new(window);
            this.mLogDic.Add(window, log);

            this.Log(window, "Managed", true);

            window.Initialized += this.Window_Initialized;
            window.Loaded += this.Window_Loaded;

            window.StateChanged += this.Window_StateChanged;
            window.SizeChanged += this.Window_SizeChanged;
            window.LocationChanged += this.Window_LocationChanged;

            window.IsVisibleChanged += this.Window_IsVisibleChanged;
            window.Closed += this.Window_Closed;
            window.Unloaded += this.Window_Unloaded;
        }

        /// <summary>
        /// ウィンドウから位置監視機能を削除する
        /// </summary>
        /// <param name="window">対象のウィンドウ</param>
        public void Remove(Window window)
        {
            this.Log(window, "Unmanaged", true);

            window.Initialized -= this.Window_Initialized;
            window.Loaded -= this.Window_Loaded;

            window.StateChanged -= this.Window_StateChanged;
            window.SizeChanged -= this.Window_SizeChanged;
            window.LocationChanged -= this.Window_LocationChanged;

            window.IsVisibleChanged -= this.Window_IsVisibleChanged;
            window.Closed -= this.Window_Closed;
            window.Unloaded -= this.Window_Unloaded;

            _ = this.mLastRectDic.Remove(window);
            _ = this.mLogDic.Remove(window);
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            if (sender is not Window window) { throw new ArgumentException("sender isn't Window"); }

            this.Log(window, "-Initialized", true);
            this.StoreInitialRect(window);
            this.StoreRect(window);

            this.LoadSetting(window);

            this.StoreRect(window);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not Window window) { throw new ArgumentException("sender isn't Window"); }

            this.Log(window, "-Loaded", true);
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (sender is not Window window) { throw new ArgumentException("sender isn't Window"); }

            this.Log(window, "-StateChanged", true);
            _ = this.ModifyLocationOrSize(window);
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is not Window window) { throw new ArgumentException("sender isn't Window"); }

            this.Log(window, "-SizeChanged", true);
            _ = this.ModifyLocationOrSize(window);
        }
        private void Window_LocationChanged(object sender, EventArgs e)
        {
            if (sender is not Window window) { throw new ArgumentException("sender isn't Window"); }

            this.Log(window, "-LocationChanged", true);
            _ = this.ModifyLocationOrSize(window);
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not Window window) { throw new ArgumentException("sender isn't Window"); }

            bool oldValue = (bool)e.OldValue;
            bool newValue = (bool)e.NewValue;
            this.Log(window, $"-IsVisibleChanged({oldValue}->{newValue})", true);
            if (oldValue != newValue) {
                if (newValue) {
                    this.LoadSetting(window);
                }
                else {
                    this.SaveSetting(window);
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (sender is not Window window) { throw new ArgumentException("sender isn't Window"); }

            this.Log(window, "-Closed", true);
            this.SaveSetting(window);
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            if (sender is not Window window) { throw new ArgumentException("sender isn't Window"); }

            this.Remove(window);
        }

        private void Log(Window window, string comment, bool forceLog = false) => this.mLogDic[window].Log(comment, forceLog);

        /// <summary>
        /// ウィンドウの初期領域を保存する
        /// </summary>
        /// <param name="window"></param>
        private void StoreInitialRect(Window window) => this.mInitialRectDic[window] = window.RestoreBounds;
        /// <summary>
        /// ウィンドウの初期領域を復元する
        /// </summary>
        /// <param name="window"></param>
        /// <returns>ウィンドウの初期領域</returns>
        private Rect RestoreInitialRect(Window window) => this.mInitialRectDic[window];

        /// <summary>
        /// ウィンドウの領域を保存する
        /// </summary>
        /// <param name="window"></param>
        private void StoreRect(Window window) => this.mLastRectDic[window] = window.RestoreBounds;
        /// <summary>
        /// ウィンドウの領域を復元する
        /// </summary>
        /// <param name="window"></param>
        /// <returns>ウィンドウの領域</returns>
        private Rect RestoreRect(Window window) => this.mLastRectDic[window];

        /// <summary>
        /// 設定を読み込む
        /// </summary>
        /// <param name="window"></param>
        private void LoadSetting(Window window)
        {
            if (window.DataContext is not WindowViewModelBase wvm) { return; }

            window.SizeChanged -= this.Window_SizeChanged;
            window.LocationChanged -= this.Window_LocationChanged;

            this.Log(window, "LoadSetting", true);

            bool isMainWindow = window is MainWindow;

            Properties.Settings settings = Properties.Settings.Default;

            Size? size = wvm.WindowSizeSetting;
            if (size is not null && 40 < size.Value.Width && 40 < size.Value.Height) {
                window.Width = size.Value.Width;
                window.Height = size.Value.Height;
            }

            Point point = wvm.WindowPointSetting;
            if ((isMainWindow || settings.App_IsPositionSaved) && 0 <= point.X && 0 <= point.Y) {
                window.Left = point.X;
                window.Top = point.Y;
            }
            else {
                window.MoveOwnersCenter();
            }

            int state = wvm.WindowStateSetting;
            if (0 < state && state <= (int)Enum.GetValues<WindowState>().Cast<WindowState>().Max()) {
                window.WindowState = (WindowState)state;
            }

            window.SizeChanged += this.Window_SizeChanged;
            window.LocationChanged += this.Window_LocationChanged;

            this.Log(window, "LoadedSetting", true);
            _ = this.ModifyLocationOrSize(window);
        }

        /// <summary>
        /// 設定を保存する
        /// </summary>
        /// <param name="window"></param>
        private void SaveSetting(Window window)
        {
            if (window.DataContext is not WindowViewModelBase wvm) { return; }

            this.Log(window, "SaveSetting", true);

            bool isMainWindow = window is MainWindow;

            if (window.WindowState == WindowState.Normal) {
                Properties.Settings settings = Properties.Settings.Default;
                if (!settings.App_InitSizeFlag) {
                    if (40 < window.Width && 40 < window.Height) {
                        Size size = new() {
                            Width = window.Width,
                            Height = window.Height
                        };
                        wvm.WindowSizeSetting = size;
                    }

                    if (isMainWindow || settings.App_IsPositionSaved) {
                        if (0 < window.Left && 0 < window.Top) {
                            Point point = new() {
                                X = window.Left,
                                Y = window.Top
                            };
                            wvm.WindowPointSetting = point;
                        }
                    }
                }
            }

            if (window.WindowState != WindowState.Minimized) {
                wvm.WindowStateSetting = (int)window.WindowState;
            }
        }

        /// <summary>
        /// ウィンドウ位置またはサイズを修正する
        /// </summary>
        /// <param name="window"></param>
        private bool ModifyLocationOrSize(Window window)
        {
            window.SizeChanged -= this.Window_SizeChanged;
            window.LocationChanged -= this.Window_LocationChanged;

            bool ret = true;
            Rect initialBounds = this.RestoreInitialRect(window);
            Rect lastBounds = this.RestoreRect(window);

            /// 位置調整
            if (30000 < Math.Max(Math.Abs(window.Left), Math.Abs(window.Top))) {
                double tmpTop = window.Top;
                double tmpLeft = window.Left;
                if (30000 < Math.Max(Math.Abs(lastBounds.Left), Math.Abs(lastBounds.Top))) {
                    window.Left = lastBounds.Left;
                    window.Top = lastBounds.Top;
                }
                else {
                    // ディスプレイの中央に移動する
                    window.MoveOwnersCenter();
                }

                if (tmpTop != window.Top || tmpLeft != window.Left) {
                    this.Log(window, "WindowLocationModified", true);
                }
                else {
                    this.Log(window, "FailedToModifyLocation", true);
                    ret = false;
                }
            }

            /// サイズ調整
            if (window.Height < 40 || window.Width < 40) {
                double tmpHeight = window.Height;
                double tmpWidth = window.Width;
                if (40 < lastBounds.Height && 40 < lastBounds.Width) {
                    window.Height = lastBounds.Height;
                    window.Width = lastBounds.Width;
                }
                else {
                    window.Height = initialBounds.Height;
                    window.Width = initialBounds.Width;
                }

                if (tmpHeight != window.Height || tmpWidth != window.Width) {
                    this.Log(window, "WindowSizeModified", true);
                }
                else {
                    this.Log(window, "FailedToModifySize", true);
                    ret = false;
                }
            }

            this.StoreRect(window);

            window.SizeChanged += this.Window_SizeChanged;
            window.LocationChanged += this.Window_LocationChanged;

            return ret;
        }
    }
}
