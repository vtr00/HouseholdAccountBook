using HouseholdAccountBook.Infrastructure.DB;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.Utilities;
using HouseholdAccountBook.Infrastructure.Utilities.Args.RequestEventArgs;
using HouseholdAccountBook.Models.AppServices;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Settings;
using HouseholdAccountBook.Views;
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
        /// DB種別セレクタVM
        /// </summary>
        public SelectorViewModel<KeyValuePair<DBKind, string>, DBKind> DBKindSelectorVM { get; } = new(static p => p.Key);

        /// <summary>
        /// PostgreSQL設定
        /// </summary>
        public PostgreSQLDBSettingViewModel PostgreSQLDBSettingVM { get; } = new();

        /// <summary>
        /// Access設定
        /// </summary>
        public OleDbSettingViewModel AccessSettingVM { get; } = new();

        /// <summary>
        /// SQLite設定
        /// </summary>
        public SQLiteSettingViewModel SQLiteSettingVM { get; init; } = new();

        #region コマンド
        /// <summary>
        /// pg_dump.exeファイル選択コマンド
        /// </summary>
        public ICommand SelectPgDumpFilePathCommand => field ??= new RelayCommand(this.SelectPgDumpFilePathCommand_Execute);
        /// <summary>
        /// pg_restore.exeファイル選択コマンド
        /// </summary>
        public ICommand SelectPgRestoreFilePathCommand => field ??= new RelayCommand(this.SelectPgRestoreFilePathCommand_Execute);
        /// <summary>
        /// DBファイル選択コマンド
        /// </summary>
        public ICommand SelectDBFilePathCommand => field ??= new RelayCommand(this.SelectDBFilePathCommand_Execute);
        #endregion
        #endregion

        #region コマンドイベントハンドラ
        /// <summary>
        /// pg_dump.exeファイル選択コマンド処理
        /// </summary>
        public void SelectPgDumpFilePathCommand_Execute()
        {
            (string directory, string fileName) = PathUtil.GetSeparatedPath(this.PostgreSQLDBSettingVM.InputedDumpExePath, App.GetCurrentDir());
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
                this.PostgreSQLDBSettingVM.InputedDumpExePath = PathUtil.GetSmartPath(App.GetCurrentDir(), e.FileName);
            }
        }

        /// <summary>
        /// pg_restore.exeファイル選択コマンド処理
        /// </summary>
        public void SelectPgRestoreFilePathCommand_Execute()
        {
            (string directory, string fileName) = PathUtil.GetSeparatedPath(this.PostgreSQLDBSettingVM.InputedRestoreExePath, App.GetCurrentDir());
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
                this.PostgreSQLDBSettingVM.InputedRestoreExePath = PathUtil.GetSmartPath(App.GetCurrentDir(), e.FileName);
            }
        }

        /// <summary>
        /// DBファイル選択コマンド処理
        /// </summary>
        public void SelectDBFilePathCommand_Execute()
        {
            string directory = string.Empty;
            string fileName = string.Empty;
            string filter = string.Empty;
            switch (this.DBKindSelectorVM.SelectedKey) {
                case DBKind.SQLite: {
                    (directory, fileName) = PathUtil.GetSeparatedPath(this.SQLiteSettingVM.InputedDBFilePath, App.GetCurrentDir());
                    filter = $"{Properties.Resources.FileSelectFilter_SQLiteFile}|*.db;*.sqlite;*.sqlite3";
                    break;
                }
                case DBKind.Access: {
                    (directory, fileName) = PathUtil.GetSeparatedPath(this.AccessSettingVM.InputedDBFilePath, App.GetCurrentDir());
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
                switch (this.DBKindSelectorVM.SelectedKey) {
                    case DBKind.SQLite:
                        this.SQLiteSettingVM.InputedDBFilePath = PathUtil.GetSmartPath(App.GetCurrentDir(), e.FileName);
                        break;
                    case DBKind.Access:
                        this.AccessSettingVM.InputedDBFilePath = PathUtil.GetSmartPath(App.GetCurrentDir(), e.FileName);
                        break;
                    default:
                        break;
                }
            }
        }

        protected override bool OKCommand_CanExecute()
        {
            bool canExecute;
            switch (this.DBKindSelectorVM.SelectedKey) {
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

        public DbSettingWindowViewModel()
        {
            using FuncLog funcLog = new();

            this.DBKindSelectorVM.SetLoader(static () => DBKindStr);
        }

        public override void Initialize(WaitCursorManagerFactory waitCursorManagerFactory, DbHandlerFactory dbHandlerFactory)
        {
            base.Initialize(waitCursorManagerFactory, dbHandlerFactory);

            this.DBKindSelectorVM.WaitCursorManagerFactory = waitCursorManagerFactory;
        }

        public override async Task LoadAsync()
        {
            using FuncLog funcLog = new();

            await this.DBKindSelectorVM.LoadAsync(UserSettingService.Instance.SelectedDBKind);

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

            switch (this.DBKindSelectorVM.SelectedKey) {
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
                UserSettingService.Instance.SelectedDBKind = this.DBKindSelectorVM.SelectedKey;
            }
            return result;
        }

        public override void AddEventHandlers()
        {
            // LOP
        }
    }
}
