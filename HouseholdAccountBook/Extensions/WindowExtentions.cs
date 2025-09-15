using HouseholdAccountBook.Models.Logger;
using HouseholdAccountBook.ViewModels.Abstract;
using Microsoft.Win32;
using System;
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
        /// <remarks>コンストラクタで呼び出す</remarks>
        public static void LoadWindowSetting(this Window window)
        {
            if (window.DataContext is not WindowViewModelBase wvm) {
                return;
            }

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
        private static void SaveWindowSetting(this Window window)
        {
            if (window.DataContext is not WindowViewModelBase wvm) {
                return;
            }

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
        /// 共通のイベントハンドラを登録する
        /// </summary>
        /// <param name="window"></param>
        /// <remarks>コンストラクタで呼び出す</remarks>
        public static void AddCommonEventHandlers(this Window window)
        {
            window.Closed += (sender, e) => {
                window.SaveWindowSetting();
            };

            if (window.DataContext is not WindowViewModelBase wvm) {
                return;
            }

            wvm.CloseRequested += (sender, e) => {
                if (e.IsDialog) {
                    try {
                        window.DialogResult = e.DialogResult;
                    }
                    catch (InvalidOperationException) { }
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
