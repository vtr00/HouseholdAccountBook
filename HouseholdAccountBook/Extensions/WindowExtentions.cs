using HouseholdAccountBook.Adapters.Logger;
using HouseholdAccountBook.ViewModels.Abstract;
using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace HouseholdAccountBook.Extensions
{
    /// <summary>
    /// <see cref="Window"/> の拡張メソッドを提供します
    /// </summary>
    public static class WindowExtentions
    {
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        /// <summary>
        /// モーダルウィンドウかどうかを判定する
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        private static bool IsModal(this Window window)
        {
            const int GWL_STYLE = -16;
            const int WS_DISABLED = 0x08000000;

            var hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd == IntPtr.Zero) return false;

            int style = GetWindowLong(hwnd, GWL_STYLE);
            return (style & WS_DISABLED) != 0;
        }

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
            Log.Debug($"window - top:{window.Top} right:{right} bottom:{bottom} left:{window.Left} width:{window.Width} height:{window.Height}");

            if (window.Owner != null) {
                double ownerRight = window.Owner.Left + window.Owner.Width;
                double ownerBottom = window.Owner.Left + window.Owner.Height;
                Log.Debug($"owner  - top:{window.Owner.Top} right:{ownerRight} bottom:{ownerBottom} left:{window.Owner.Left} width:{window.Owner.Width} height:{window.Owner.Height}");
            }
        }

        /// <summary>
        /// <see cref="WindowViewModelBase"/> に汎用のイベントハンドラを登録する
        /// </summary>
        /// <param name="window"></param>
        /// <remarks>コンストラクタで呼び出す</remarks>
        public static void AddCommonEventHandlersToVM(this Window window)
        {
            if (window.DataContext is not WindowViewModelBase wvm) {
                return;
            }

            /// ViewModelにWindow関連のイベントハンドラを登録する
            wvm.CloseRequested += (sender, e) => {
                if (window.IsModal()) {
                    try {
                        window.DialogResult = e.Result;
                    }
                    catch (InvalidOperationException) { }
                }
                window.Close();
            };
            wvm.HideRequested += (sender, e) => window.Hide();

            wvm.OpenFileDialogRequested += (sender, e) => {
                OpenFileDialog ofd = new() {
                    CheckFileExists = e.CheckFileExists,
                    InitialDirectory = e.InitialDirectory,
                    FileName = e.FileName,
                    Title = e.Title,
                    Filter = e.Filter,
                    Multiselect = e.Multiselect,
                    CheckPathExists = e.CheckPathExists
                };

                bool? result = ofd.ShowDialog(window) ?? throw new InvalidOperationException();
                e.Result = (bool)result;
                if (e.Result) {
                    switch (e.Multiselect) {
                        case false:
                            e.FileName = ofd.FileName;
                            break;
                        case true:
                            e.FileNames = ofd.FileNames;
                            break;
                    }
                }
            };
            wvm.OpenFolderDialogRequested += (sender, e) => {
                OpenFolderDialog ofd = new() {
                    InitialDirectory = e.InitialDirectory,
                    Title = e.Title,
                };

                bool? result = ofd.ShowDialog(window) ?? throw new InvalidOperationException();
                e.Result = (bool)result;
                if (e.Result) {
                    e.FolderName = ofd.FolderName;
                }
            };
            wvm.SaveFileDialogRequested += (sender, e) => {
                SaveFileDialog ofd = new() {
                    InitialDirectory = e.InitialDirectory,
                    FileName = e.FileName,
                    Title = e.Title,
                    Filter = e.Filter,
                };

                bool? result = ofd.ShowDialog(window) ?? throw new InvalidOperationException();
                e.Result = (bool)result;
                if (e.Result) {
                    e.FileName = ofd.FileName;
                }
            };
        }
    }
}
