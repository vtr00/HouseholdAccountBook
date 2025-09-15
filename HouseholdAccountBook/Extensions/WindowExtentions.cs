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
        /// <remarks>通常ウィンドウクローズ時に呼ばれる</remarks>
        public static void SaveWindowSetting(this Window window)
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
            /// ウィンドウのイベントハンドラを登録する
            window.Closed += (sender, e) => {
                window.SaveWindowSetting();
            };
            window.IsVisibleChanged += (sender, e) => {
                bool oldValue = (bool)e.OldValue;
                bool newValue = (bool)e.NewValue;
                if (!oldValue && newValue) {
                    if (newValue) {
                        window.LoadWindowSetting();
                    }
                    else {
                        window.SaveWindowSetting();
                    }
                }
            };

            if (window.DataContext is not WindowViewModelBase wvm) {
                return;
            }

            /// ViewModelのイベントハンドラを登録する
            wvm.CloseRequested += (sender, e) => {
                if (e.IsDialog) {
                    try {
                        window.DialogResult = e.DialogResult;
                    }
                    catch (InvalidOperationException) { }
                }
                window.Close();
            };
            wvm.HideRequested += (sender, e) => {
                window.Hide();
            };

            wvm.OpenFileDialogRequested += (sender, e) => {
                OpenFileDialog ofd = new() {
                    CheckFileExists = true,
                    InitialDirectory = e.InitialDirectory,
                    FileName = e.FileName,
                    Title = e.Title,
                    Filter = e.Filter,
                    Multiselect = e.Multiselect
                };

                e.Result = ofd.ShowDialog(window);
                if (e.Result == true) {
                    switch(e.Multiselect) {
                        case false:
                            e.FileName = ofd.FileName;
                            break;
                        case true:
                            e.FileNames = ofd.FileNames;
                            break;
                    }
                }
            };
        }
    }
}
