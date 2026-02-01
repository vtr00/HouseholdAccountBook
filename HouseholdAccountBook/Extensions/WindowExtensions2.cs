using HouseholdAccountBook.Adapters.Logger;
using HouseholdAccountBook.ViewModels.Abstract;
using Microsoft.Win32;
using System;
using System.Windows;

namespace HouseholdAccountBook.Extensions
{
    /// <summary>
    /// <see cref="Window"/> の拡張メソッドを提供します
    /// </summary>
    public static class WindowExtensions2
    {
        /// <summary>
        /// <see cref="WindowViewModelBase"/> に汎用のイベントハンドラを登録する
        /// </summary>
        /// <param name="window"></param>
        /// <remarks>コンストラクタで呼び出す</remarks>
        public static void AddCommonEventHandlersToVM(this Window window)
        {
            if (window.DataContext is not WindowViewModelBase wvm) { return; }

            using FuncLog funcLog = new(new { WindowName = window.Name });

            /// ViewModelにWindow関連のイベントハンドラを登録する
            wvm.CloseRequested += async (sender, e) => {
                using FuncLog funcLog = new(new { WindowName = window.Name }, methodName: nameof(wvm.CloseRequested));

                if (window.GetIsModal()) {
                    try {
                        window.DialogResult = e.Result;
                    }
                    catch (InvalidOperationException) {
                        Log.Warning("Failed to set DialogResult");
                    }
                }
                window.Close();
            };
            wvm.HideRequested += (sender, e) => window.Hide();

            wvm.OpenFileDialogRequested += (sender, e) => {
                using FuncLog funcLog = new(new { WindowName = window.Name }, methodName: nameof(wvm.OpenFileDialogRequested));

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
                using FuncLog funcLog = new(new { WindowName = window.Name }, methodName: nameof(wvm.OpenFolderDialogRequested));

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
                using FuncLog funcLog = new(new { WindowName = window.Name }, methodName: nameof(wvm.SaveFileDialogRequested));

                SaveFileDialog sfd = new() {
                    InitialDirectory = e.InitialDirectory,
                    FileName = e.FileName,
                    Title = e.Title,
                    Filter = e.Filter,
                };

                bool? result = sfd.ShowDialog(window) ?? throw new InvalidOperationException();
                e.Result = (bool)result;
                if (e.Result) {
                    e.FileName = sfd.FileName;
                }
            };
        }
    }
}
