using HouseholdAccountBook.Models.Logger;
using HouseholdAccountBook.ViewModels.Abstract;
using Microsoft.Win32;
using System.Windows;

namespace HouseholdAccountBook.Extensions
{
    /// <summary>
    /// <see cref="Window"/> の拡張メソッドを提供します
    /// </summary>
    public static class WindowExtentions
    {
        /// <summary>
        /// <see cref="Window.Owner"/> の中央位置に移動する
        /// </summary>
        /// <param name="window"></param>
        public static void MoveOwnersCenter(this Window window)
        {
            window.WindowStartupLocation = window.Owner != null && window.Owner.WindowState == WindowState.Normal
                ? WindowStartupLocation.CenterOwner
                : WindowStartupLocation.CenterScreen;

            double right = window.Left + window.Width;
            double bottom = window.Top + window.Height;
            Log.Info(string.Format($"window - top:{window.Top} right:{right} bottom:{bottom} left:{window.Left} width:{window.Width} height:{window.Height}"));

            if (window.Owner != null) {
                double OwnerRight = window.Owner.Left + window.Owner.Width;
                double OwnerBottom = window.Owner.Left + window.Owner.Height;
                Log.Info(string.Format($"owner  - top:{window.Owner.Top} right:{OwnerRight} bottom:{OwnerBottom} left:{window.Owner.Left} width:{window.Owner.Width} height:{window.Owner.Height}"));
            }
        }

        /// <summary>
        /// ウィンドウ設定を読み込む
        /// </summary>
        /// <param name="window"></param>
        /// <param name="wvm"></param>
        public static void LoadWindowSetting(this Window window, WindowViewModelBase wvm)
        {
            Size? size = wvm.WindowSizeSetting;
            if (size is not null) {
                window.Width = size.Value.Width;
                window.Height = size.Value.Height;
            }

            Point? point = wvm.WindowPointSetting;
            if (point is not null) {
                window.Left = point.Value.X;
                window.Top = point.Value.Y;
            }
            else {
                window.MoveOwnersCenter();
            }
        }

        /// <summary>
        /// ウィンドウ設定を保存する
        /// </summary>
        /// <param name="window"></param>
        /// <param name="wvm"></param>
        public static void SaveWindowSetting(this Window window, WindowViewModelBase wvm)
        {
            if (window.WindowState == WindowState.Normal) {
                Rect rect = new() {
                    X = window.Left,
                    Y = window.Top,
                    Width = window.Width,
                    Height = window.Height
                };
                wvm.WindowRectSetting = rect;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="window"></param>
        /// <param name="wvm"></param>
        public static void RegisterEventHandlers(this Window window, WindowViewModelBase wvm)
        {
            window.Initialized += (sender, e) => {
                window.LoadWindowSetting(wvm);
            };
            window.Closed += (sender, e) => {
                window.SaveWindowSetting(wvm);
            };
            wvm.CloseRequested += (sender, e) => {
                if (e.IsDialog) {
                    window.DialogResult = e.DialogResult;
                }
                window.Close();
            };
            wvm.OpenFileDialogRequested += (sender, e) => {
                OpenFileDialog ofd = new() {
                    CheckFileExists = true,
                    InitialDirectory = e.InitialDirectory,
                    FileName = e.FileName,
                    Title = e.Title,
                    Filter = e.Filter
                };

                e.Result = ofd.ShowDialog(window);
                if (e.Result == true) {
                    e.FileName = ofd.FileName;
                }
            };
        }
    }
}
