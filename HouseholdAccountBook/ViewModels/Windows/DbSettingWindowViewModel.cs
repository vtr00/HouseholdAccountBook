using HouseholdAccountBook.Infrastructure.DB;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.Utilities;
using HouseholdAccountBook.Models.AppServices;
using HouseholdAccountBook.Models.Args.RequestEventArgs;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Common;
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
        public string Message {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// DB種別リストVM
        /// </summary>
        public SelectableViewModel<KeyValuePair<DBKind, string>, DBKind> DBKindListVM => field ??= new(static p => p.Key);

        /// <summary>
        /// PostgreSQL設定
        /// </summary>
        public PostgreSQLSettingViewModel PostgreSQLDBSettingVM { get; init; } = new();

        /// <summary>
        /// Access設定
        /// </summary>
        public OleDbSettingViewModel AccessSettingVM { get; init; } = new();

        /// <summary>
        /// SQLite設定
        /// </summary>
        public SQLiteSettingViewModel SQLiteSettingVM { get; init; } = new();
        #endregion

        #region コマンド
        /// <summary>
        /// pg_dump.exeファイル選択コマンド
        /// </summary>
        public ICommand SelectPgDumpFilePathCommand => field ??= new RelayCommand(this.SelectPgDumpFilePathCommand_Execute);
        /// <summary>
        /// pg_dump.exeファイル選択コマンド処理
        /// </summary>
        public void SelectPgDumpFilePathCommand_Execute()
        {
            (string directory, string fileName) = PathUtil.GetSeparatedPath(this.PostgreSQLDBSettingVM.DumpExePathVM.InputedValue, App.GetCurrentDir());
            if (string.IsNullOrWhiteSpace(directory)) {
                directory = App.GetCurrentDir();
            }

            OpenFileDialogRequestEventArgs e = new() {
                CheckFileExists = true,
                InitialDirectory = directory,
                FileName = fileName,
                Title = Properties.Resources.Title_FileSelection,
                Filter = "pg_dump.exe|pg_dump.exe",
            };
            if (this.OpenFileDialogRequest(e)) {
                this.PostgreSQLDBSettingVM.DumpExePathVM.InputedValue = PathUtil.GetSmartPath(App.GetCurrentDir(), e.FileName);
            }
        }

        /// <summary>
        /// pg_restore.exeファイル選択コマンド
        /// </summary>
        public ICommand SelectPgRestoreFilePathCommand => field ??= new RelayCommand(this.SelectPgRestoreFilePathCommand_Execute);
        /// <summary>
        /// pg_restore.exeファイル選択コマンド処理
        /// </summary>
        public void SelectPgRestoreFilePathCommand_Execute()
        {
            (string directory, string fileName) = PathUtil.GetSeparatedPath(this.PostgreSQLDBSettingVM.RestoreExePathVM.InputedValue, App.GetCurrentDir());
            if (string.IsNullOrWhiteSpace(directory)) {
                directory = App.GetCurrentDir();
            }

            OpenFileDialogRequestEventArgs e = new() {
                CheckFileExists = true,
                InitialDirectory = directory,
                FileName = fileName,
                Title = Properties.Resources.Title_FileSelection,
                Filter = "pg_restore.exe|pg_restore.exe",
            };
            if (this.OpenFileDialogRequest(e)) {
                this.PostgreSQLDBSettingVM.RestoreExePathVM.InputedValue = PathUtil.GetSmartPath(App.GetCurrentDir(), e.FileName);
            }
        }

        /// <summary>
        /// DBファイル選択コマンド
        /// </summary>
        public ICommand SelectDBFilePathCommand => field ??= new RelayCommand(this.SelectDBFilePathCommand_Execute);
        /// <summary>
        /// DBファイル選択コマンド処理
        /// </summary>
        public void SelectDBFilePathCommand_Execute()
        {
            string directory = string.Empty;
            string fileName = string.Empty;
            string filter = string.Empty;
            switch (this.DBKindListVM.SelectedKey) {
                case DBKind.SQLite: {
                    (directory, fileName) = PathUtil.GetSeparatedPath(this.SQLiteSettingVM.DBFilePathVM.InputedValue, App.GetCurrentDir());
                    filter = $"{Properties.Resources.FileSelectFilter_SQLiteFile}|*.db;*.sqlite;*.sqlite3";
                    break;
                }
                case DBKind.Access: {
                    (directory, fileName) = PathUtil.GetSeparatedPath(this.AccessSettingVM.DBFilePathVM.InputedValue, App.GetCurrentDir());
                    filter = $"{Properties.Resources.FileSelectFilter_AccessFile}|*.mdb;*.accdb";
                    break;
                }
                default:
                    break;
            }

            OpenFileDialogRequestEventArgs e = new() {
                CheckFileExists = false,
                InitialDirectory = directory,
                FileName = fileName,
                Title = Properties.Resources.Title_FileSelection,
                Filter = filter,
            };
            if (this.OpenFileDialogRequest(e)) {
                switch (this.DBKindListVM.SelectedKey) {
                    case DBKind.SQLite:
                        this.SQLiteSettingVM.DBFilePathVM.InputedValue = PathUtil.GetSmartPath(App.GetCurrentDir(), e.FileName);
                        break;
                    case DBKind.Access:
                        this.AccessSettingVM.DBFilePathVM.InputedValue = PathUtil.GetSmartPath(App.GetCurrentDir(), e.FileName);
                        break;
                    default:
                        break;
                }
            }
        }

        protected override bool OKCommand_CanExecute()
        {
            bool canExecute;
            switch (this.DBKindListVM.SelectedKey) {
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
        protected override void OKCommand_Execute()
        {
            bool result = this.Save();

            if (result) {
                base.OKCommand_Execute();
            }
        }
        #endregion

        #region ウィンドウ設定プロパティ
        protected override (double, double) WindowSizeSettingRaw {
            get => UserSettingService.Instance.DbSettingWindowSize;
            set => UserSettingService.Instance.DbSettingWindowSize = value;
        }

        public override Point WindowPointSetting {
            get => UserSettingService.Instance.DbSettingWindowPoint;
            set => UserSettingService.Instance.DbSettingWindowPoint = value;
        }
        #endregion

        public override void Initialize(DbHandlerFactory dbHandlerFactory)
        {
            base.Initialize(dbHandlerFactory);

            this.DBKindListVM.SetLoader(static () => DBKindStr);
        }

        public override async Task LoadAsync()
        {
            using FuncLog funcLog = new();
            using IDisposable disposable = this.mBusyService.Enter();

            await this.DBKindListVM.LoadAsync(UserSettingService.Instance.SelectedDBKind);

            // PostgreSQL
            this.PostgreSQLDBSettingVM.Load(this.SetPassword);
            // SQLite
            this.SQLiteSettingVM.Load();
            // Access
            await this.AccessSettingVM.LoadAsync();
        }

        /// <summary>
        /// 保存処理
        /// </summary>
        /// <returns>保存結果</returns>
        public bool Save()
        {
            bool result = false;

            switch (this.DBKindListVM.SelectedKey) {
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
                    result = false;
                    break;
            }
            if (result) {
                UserSettingService.Instance.SelectedDBKind = this.DBKindListVM.SelectedKey;
            }
            return result;
        }

        public override void AddEventHandlers()
        {
            // LOP
        }
    }
}
