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
        private readonly Dictionary<Window, WindowLog> logDic = [];
        /// <summary>
        /// 初期値
        /// </summary>
        private readonly Dictionary<Window, Rect> initialRectDic = [];
        /// <summary>
        /// 最終補正値
        /// </summary>
        private readonly Dictionary<Window, Rect> lastRectDic = [];

        static WindowLocationManager() => Register(static () => new WindowLocationManager());

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

            if (this.logDic.ContainsKey(window)) return;

            var log = new WindowLog(window);
            this.logDic.Add(window, log);

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

            _ = this.lastRectDic.Remove(window);
            _ = this.logDic.Remove(window);
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            this.Log(sender, "-Initialized", true);
            this.StoreInitialRect(sender);
            this.StoreRect(sender);

            this.LoadSetting(sender);

            this.StoreRect(sender);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Log(sender, "-Loaded", true);
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            this.Log(sender, "-WindowStateChanged", true);
            _ = this.ModifyLocationOrSize(sender);
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Log(sender, "-WindowSizeChanged", true);
            _ = this.ModifyLocationOrSize(sender);
        }
        private void Window_LocationChanged(object sender, EventArgs e)
        {
            this.Log(sender, "-WindowLocationChanged", true);
            _ = this.ModifyLocationOrSize(sender);
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            bool oldValue = (bool)e.OldValue;
            bool newValue = (bool)e.NewValue;
            this.Log(sender, $"-IsVisibleChanged({oldValue}->{newValue})", true);
            if (oldValue != newValue) {
                if (newValue) {
                    this.LoadSetting(sender);
                }
                else {
                    this.SaveSetting(sender);
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.SaveSetting(sender);
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            if (sender is not Window window) throw new ArgumentException("sender is not Window");

            this.Remove(window);
        }

        private void Log(object sender, string comment, bool forceLog = false)
        {
            if (sender is not Window window) throw new ArgumentException("sender is not Window");

            this.logDic[window].Log(comment, forceLog);
        }

        private void StoreInitialRect(object sender)
        {
            if (sender is not Window window) throw new ArgumentException("sender is not Window");

            this.initialRectDic[window] = window.RestoreBounds;
        }
        private Rect RestoreInitialRect(object sender)
        {
            if (sender is not Window window) throw new ArgumentException("sender is not Window");

            return this.initialRectDic[window];
        }

        private void StoreRect(object sender)
        {
            if (sender is not Window window) throw new ArgumentException("sender is not Window");

            this.lastRectDic[window] = window.RestoreBounds;
        }
        private Rect RestoreRect(object sender)
        {
            if (sender is not Window window) throw new ArgumentException("sender is not Window");

            return this.lastRectDic[window];
        }

        /// <summary>
        /// 設定を読み込む
        /// </summary>
        /// <param name="sender"></param>
        private void LoadSetting(object sender)
        {
            if (sender is not Window window) throw new ArgumentException("sender is not Window");
            if (window.DataContext is not WindowViewModelBase wvm) return;

            window.SizeChanged -= this.Window_SizeChanged;
            window.LocationChanged -= this.Window_LocationChanged;

            this.Log(sender, "LoadSetting", true);

            bool isMainWindow = window is MainWindow;

            Properties.Settings settings = Properties.Settings.Default;

            Size size = wvm.WindowSizeSetting;
            if (40 < size.Width && 40 < size.Height) {
                window.Width = size.Width;
                window.Height = size.Height;
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
            if (0 < state && state <= (int)Enum.GetValues(typeof(WindowState)).Cast<WindowState>().Max()) {
                window.WindowState = (WindowState)state;
            }

            window.SizeChanged += this.Window_SizeChanged;
            window.LocationChanged += this.Window_LocationChanged;

            this.Log(sender, "LoadedSetting", true);
            _ = this.ModifyLocationOrSize(sender);
        }

        /// <summary>
        /// 設定を保存する
        /// </summary>
        /// <param name="sender"></param>
        private void SaveSetting(object sender)
        {
            if (sender is not Window window) throw new ArgumentException("sender is not Window");
            if (window.DataContext is not WindowViewModelBase wvm) return;

            this.Log(sender, "SaveSetting", true);

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
        private bool ModifyLocationOrSize(object sender)
        {
            if (sender is not Window window) throw new ArgumentException("sender is not Window");

            window.SizeChanged -= this.Window_SizeChanged;
            window.LocationChanged -= this.Window_LocationChanged;

            bool ret = true;
            Rect initialBounds = this.RestoreInitialRect(sender);
            Rect lastBounds = this.RestoreRect(sender);

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
                    this.Log(sender, "WindowLocationModified", true);
                }
                else {
                    this.Log(sender, "FailedToModifyLocation", true);
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
                    this.Log(sender, "WindowSizeModified", true);
                }
                else {
                    this.Log(sender, "FailedToModifySize", true);
                    ret = false;
                }
            }

            this.StoreRect(sender);

            window.SizeChanged += this.Window_SizeChanged;
            window.LocationChanged += this.Window_LocationChanged;

            return ret;
        }
    }
}
