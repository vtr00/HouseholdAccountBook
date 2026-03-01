using HouseholdAccountBook.Models;
using HouseholdAccountBook.Models.Infrastructure;
using HouseholdAccountBook.Models.Infrastructure.Logger;
using HouseholdAccountBook.Models.Utilities.Args.RequestEventArgs;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Settings;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static HouseholdAccountBook.ViewModels.UiConstants;

namespace HouseholdAccountBook.ViewModels.Windows
{
    /// <summary>
    /// DB設定ウィンドウVM
    /// </summary>
    public class DbSettingWindowViewModel : WindowViewModelBase
    {
        /// <summary>
        /// パスワードを設定する
        /// </summary>
        public Action<string> SetPassword;
        /// <summary>
        /// パスワードを取得する
        /// </summary>
        public Func<string> GetPassword;

        #region Bindingプロパティ
        /// <summary>
        /// 表示メッセージ
        /// </summary>
        #region Message
        public string Message {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// DB種別辞書
        /// </summary>
        #region DBKindDic
        public Dictionary<DBKind, string> DBKindDic { get; } = DBKindStr;
        #endregion
        /// <summary>
        /// 選択されたDB種別
        /// </summary>
        #region SelectedDBKind
        public DBKind SelectedDBKind {
            get;
            set => this.SetProperty(ref field, value);
        } = DBKind.PostgreSQL;
        #endregion

        /// <summary>
        /// PostgreSQL設定
        /// </summary>
        #region PostgreSQLDBSettingVM
        public PostgreSQLDBSettingViewModel PostgreSQLDBSettingVM {
            get;
            set => this.SetProperty(ref field, value);
        } = new();
        #endregion

        /// <summary>
        /// Access設定
        /// </summary>
        #region AccessSettingVM
        public OleDbSettingViewModel AccessSettingVM {
            get;
            set => this.SetProperty(ref field, value);
        } = new();
        #endregion

        /// <summary>
        /// SQLite設定
        /// </summary>
        #region SQLiteSettingVM
        public SQLiteSettingViewModel SQLiteSettingVM {
            get;
            set => this.SetProperty(ref field, value);
        } = new();
        #endregion

        #region コマンド
        public override ICommand SelectFilePathCommand => new RelayCommand<FilePathKind>(this.SelectFilePathCommand_Executed);
        #endregion
        #endregion

        #region コマンドイベントハンドラ
        /// <summary>
        /// ファイル選択コマンド処理
        /// </summary>
        /// <param name="kind">ファイルパス種別</param>
        public void SelectFilePathCommand_Executed(FilePathKind kind)
        {
            bool checkFileExists = true;
            string directory = string.Empty;
            string fileName = string.Empty;
            string filter = string.Empty;
            switch (kind) {
                case FilePathKind.DumpExeFile: {
                    (directory, fileName) = PathUtil.GetSeparatedPath(this.PostgreSQLDBSettingVM.InputedDumpExePath, App.GetCurrentDir());
                    if (string.IsNullOrWhiteSpace(directory)) {
                        directory = App.GetCurrentDir();
                    }
                    filter = "pg_dump.exe|pg_dump.exe";
                    break;
                }
                case FilePathKind.RestoreExeFile: {
                    (directory, fileName) = PathUtil.GetSeparatedPath(this.PostgreSQLDBSettingVM.InputedRestoreExePath, App.GetCurrentDir());
                    if (string.IsNullOrWhiteSpace(directory)) {
                        directory = App.GetCurrentDir();
                    }
                    filter = "pg_restore.exe|pg_restore.exe";
                    break;
                }
                case FilePathKind.DbFile: {
                    switch (this.SelectedDBKind) {
                        case DBKind.SQLite: {
                            checkFileExists = false;
                            (directory, fileName) = PathUtil.GetSeparatedPath(this.SQLiteSettingVM.InputedDBFilePath, App.GetCurrentDir());
                            filter = $"{Properties.Resources.FileSelectFilter_SQLiteFile}|*.db;*.sqlite;*.sqlite3";
                            break;
                        }
                        case DBKind.Access: {
                            checkFileExists = false;
                            (directory, fileName) = PathUtil.GetSeparatedPath(this.AccessSettingVM.InputedDBFilePath, App.GetCurrentDir());
                            filter = $"{Properties.Resources.FileSelectFilter_AccessFile}|*.mdb;*.accdb";
                            break;
                        }
                    }
                    break;
                }
            }

            OpenFileDialogRequestEventArgs e = new() {
                CheckFileExists = checkFileExists,
                InitialDirectory = directory,
                FileName = fileName,
                Title = Properties.Resources.Title_FileSelection,
                Filter = filter,
            };
            if (this.OpenFileDialogRequest(e)) {
                switch (kind) {
                    case FilePathKind.DumpExeFile:
                        this.PostgreSQLDBSettingVM.InputedDumpExePath = PathUtil.GetSmartPath(App.GetCurrentDir(), e.FileName);
                        break;
                    case FilePathKind.RestoreExeFile:
                        this.PostgreSQLDBSettingVM.InputedRestoreExePath = PathUtil.GetSmartPath(App.GetCurrentDir(), e.FileName);
                        break;
                    case FilePathKind.DbFile:
                        switch (this.SelectedDBKind) {
                            case DBKind.SQLite:
                                this.SQLiteSettingVM.InputedDBFilePath = PathUtil.GetSmartPath(App.GetCurrentDir(), e.FileName);
                                break;
                            case DBKind.Access:
                                this.AccessSettingVM.InputedDBFilePath = PathUtil.GetSmartPath(App.GetCurrentDir(), e.FileName);
                                break;
                        }
                        break;
                }
            }
        }

        protected override bool OKCommand_CanExecute()
        {
            bool canExecute;
            switch (this.SelectedDBKind) {
                case DBKind.SQLite: {
                    canExecute = this.SQLiteSettingVM.CanSave();
                    break;
                }
                case DBKind.PostgreSQL: {
                    canExecute = this.PostgreSQLDBSettingVM.CanSave(this.GetPassword);
                    break;
                }
                case DBKind.Access: {
                    canExecute = this.AccessSettingVM.CanSave();
                    break;
                }
                case DBKind.Undefined:
                default:
                    canExecute = false;
                    break;
            }
            return canExecute;
        }

        protected override void OKCommand_Executed()
        {
            bool result = this.Save();

            if (result) {
                base.OKCommand_Executed();
            }
        }
        #endregion

        #region ウィンドウ設定プロパティ
        protected override (double, double) WindowSizeSettingRaw {
            get {
                Properties.Settings settings = Properties.Settings.Default;
                return (settings.DbSettingWindow_Width, settings.DbSettingWindow_Height);
            }
            set {
                Properties.Settings settings = Properties.Settings.Default;
                settings.DbSettingWindow_Width = value.Item1;
                settings.DbSettingWindow_Height = value.Item2;
                settings.Save();
            }
        }

        public override Point WindowPointSetting {
            get {
                Properties.Settings settings = Properties.Settings.Default;
                return new Point(settings.DbSettingWindow_Left, settings.DbSettingWindow_Top);
            }
            set {
                Properties.Settings settings = Properties.Settings.Default;
                settings.DbSettingWindow_Left = value.X;
                settings.DbSettingWindow_Top = value.Y;
                settings.Save();
            }
        }
        #endregion

        public override Task LoadAsync() => throw new NotImplementedException();

        public void Load()
        {
            using FuncLog funcLog = new();

            Properties.Settings settings = Properties.Settings.Default;
            this.SelectedDBKind = (DBKind)settings.App_SelectedDBKind;

            // PostgreSQL
            this.PostgreSQLDBSettingVM.Load(this.SetPassword);

            // SQLite
            this.SQLiteSettingVM.Load();

            // Access
            this.AccessSettingVM.Load();
        }

        /// <summary>
        /// 保存処理
        /// </summary>
        /// <returns>保存結果</returns>
        public bool Save()
        {
            bool result = false;
            Properties.Settings settings = Properties.Settings.Default;
            settings.App_SelectedDBKind = (int)this.SelectedDBKind;

            switch (this.SelectedDBKind) {
                case DBKind.PostgreSQL: {
                    result = this.PostgreSQLDBSettingVM.Save(this.GetPassword);
                    break;
                }
                case DBKind.SQLite: {
                    result = this.SQLiteSettingVM.Save();
                    break;
                }
                case DBKind.Access: {
                    result = this.AccessSettingVM.Save();
                    break;
                }
                case DBKind.Undefined:
                default:
                    break;
            }
            if (result) {
                settings.Save();
            }
            return result;
        }

        public override void AddEventHandlers()
        {
            // LOP
        }
    }
}
