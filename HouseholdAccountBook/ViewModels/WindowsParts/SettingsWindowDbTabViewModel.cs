using HouseholdAccountBook.Infrastructure.DB;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.Services;
using HouseholdAccountBook.Infrastructure.Utilities;
using HouseholdAccountBook.Models.AppServices;
using HouseholdAccountBook.Models.Args.RequestEventArgs;
using HouseholdAccountBook.Models.DbHandlers;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Common;
using HouseholdAccountBook.ViewModels.Settings;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HouseholdAccountBook.ViewModels.WindowsParts
{
    /// <summary>
    /// DB設定タブVM
    /// </summary>
    public class SettingsWindowDbTabViewModel : WindowPartViewModelBase
    {
        #region イベント
        /// <summary>
        /// 更新必要時イベント
        /// </summary>
        public event EventHandler NeedToUpdateChanged;
        #endregion

        #region Bindingプロパティ
        #region データベース
        /// <summary>
        /// DB種別VM
        /// </summary>
        public InputableViewModel<DBKind> DBKindVM { get; } = new();
        /// <summary>
        /// PostgreSQL設定
        /// </summary>
        public PostgreSQLSettingViewModel PostgreSQLDBSettingVM { get; } = new();
        /// <summary>
        /// Access設定
        /// </summary>
        public OleDbSettingViewModel AccessSettingVM { get; } = new();
        /// <summary>
        /// SQLite設定
        /// </summary>
        public SQLiteSettingViewModel SQLiteSettingVM { get; } = new();

        /// <summary>
        /// DB設定未保存状態マネージャ
        /// </summary>
        public DirtyStatusManager DBSettingDirtyStatus => field ??= new(this.DBKindVM, this.PostgreSQLDBSettingVM.DirtyStatus, this.AccessSettingVM.DirtyStatus, this.SQLiteSettingVM.DirtyStatus);
        #endregion

        #region バックアップ
        /// <summary>
        /// バックアップ数VM
        /// </summary>
        public InputableViewModel<int> BackUpNumVM { get; } = new();
        /// <summary>
        /// バックアップ先フォルダVM
        /// </summary>
        public InputableViewModel<string> BackUpFolderPathVM { get; } = new();
        /// <summary>
        /// メインウィンドウ最小化時バックアップ設定VM
        /// </summary>
        public InputableViewModel<bool> IfBackUpAtMinimizingVM { get; } = new();
        /// <summary>
        /// メインウィンドウ最小化時バックアップインターバルVM(分)
        /// </summary>
        public InputableViewModel<int> BackUpIntervalAtMinimizingVM { get; } = new();
        /// <summary>
        /// メインウィンドウクローズ時バックアップ設定VM
        /// </summary>
        public InputableViewModel<bool> IfBackUpAtClosingVM { get; } = new();
        /// <summary>
        /// バックアップ条件VM
        /// </summary>
        public InputableViewModel<BackUpCondition> BackUpConditionVM { get; } = new();
        /// <summary>
        /// メインウィンドウ最小化時バックアップ通知設定VM
        /// </summary>
        public InputableViewModel<bool> IfNotifyBackUpAtMinimizingVM { get; } = new();

        /// <summary>
        /// バックアップ設定未保存状態マネージャ
        /// </summary>
        public DirtyStatusManager BackupSettingDirtyStatus => field ??=
            new(this.BackUpNumVM, this.BackUpFolderPathVM,
                this.IfBackUpAtMinimizingVM, this.BackUpIntervalAtMinimizingVM, this.IfBackUpAtClosingVM, this.BackUpConditionVM, this.IfNotifyBackUpAtMinimizingVM);
        #endregion

        /// <summary>
        /// 未保存状態マネージャ
        /// </summary>
        public DirtyStatusManager DirtyState => field ??= new(this.DBSettingDirtyStatus, this.BackupSettingDirtyStatus);
        #endregion

        #region コマンド
        /// <summary>
        /// pg_dump.exeファイル選択コマンド
        /// </summary>
        public ICommand SelectPgDumpFilePathCommand => field ??= new RelayCommand(this.SelectPgDumpFilePathCommand_Execute);
        /// <summary>
        /// ファイル選択コマンド処理
        /// </summary>
        private void SelectPgDumpFilePathCommand_Execute()
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
        /// ファイル選択コマンド処理
        /// </summary>
        private void SelectPgRestoreFilePathCommand_Execute()
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
        /// データベース再設定コマンド
        /// </summary>
        public ICommand RestartForDbSettingCommand => field ??= new RelayCommand(this.RestartForDbSettingCommand_Execute);
        /// <summary>
        /// データベース設定コマンド処理
        /// </summary>
        private void RestartForDbSettingCommand_Execute()
        {
            if (MessageBox.Show(Properties.Resources.Message_RestartNotification, Properties.Resources.Title_Conformation,
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK) {
                UserSettingService.Instance.InitFlag = true;

                ((App)Application.Current).Restart();
            }
        }

        /// <summary>
        /// CSVフォルダ選択コマンド
        /// </summary>
        public ICommand SelectCsvFolderPathCommand => field ??= new RelayCommand(this.SelectCsvFolderPathCommand_Execute);
        /// <summary>
        /// CSVフォルダ選択コマンド処理
        /// </summary>
        private void SelectCsvFolderPathCommand_Execute()
        {
            string folderFullPath;
            if (string.IsNullOrWhiteSpace(this.BackUpFolderPathVM.InputedValue)) {
                folderFullPath = App.GetCurrentDir();
            }
            else {
                (string folderPath, string fileName) = PathUtil.GetSeparatedPath(this.BackUpFolderPathVM.InputedValue, App.GetCurrentDir());
                folderFullPath = Path.Combine(folderPath, fileName);
            }

            OpenFolderDialogRequestEventArgs e = new() {
                InitialDirectory = folderFullPath,
                Title = Properties.Resources.Title_BackupFolderSelection
            };
            if (this.OpenFolderDialogRequest(e)) {
                this.BackUpFolderPathVM.InputedValue = PathUtil.GetSmartPath(App.GetCurrentDir(), e.FolderName);
            }
        }

        /// <summary>
        /// DB設定保存コマンド
        /// </summary>
        public ICommand SaveDbSettingsCommand => field ??= new AsyncRelayCommand(this.SaveDbSettingsCommand_Execute, () => this.DirtyState.IsDirty);
        /// <summary>
        /// DB設定保存コマンド処理
        /// </summary>
        private async Task SaveDbSettingsCommand_Execute()
        {
            this.Save();
            // 再読込する
            await this.LoadAsync();

            _ = MessageBox.Show(Properties.Resources.Message_CompletedToSave, Properties.Resources.Title_Information, MessageBoxButton.OK, MessageBoxImage.Information);
            this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="busyService">処理中状態サービス</param>
        public SettingsWindowDbTabViewModel(BusyService busyService) : base(busyService)
        {
            using FuncLog funcLog = new();
        }

        /// <summary>
        /// DB設定を読み込む
        /// </summary>
        public override async Task LoadAsync()
        {
            using FuncLog funcLog = new();

            this.DBKindVM.LoadedValue = UserSettingService.Instance.SelectedDBKind;

            // SQLite
            this.SQLiteSettingVM.Load();
            // PostgreSQL
            this.PostgreSQLDBSettingVM.Load(null);
            // Access(記帳風月)
            await this.AccessSettingVM.LoadForKichoFugetsuAsync();

            // バックアップ
            DbBackUpService.Configuration config = UserSettingService.Instance.DbBackupConfig;
            this.BackUpNumVM.LoadedValue = config.Amount;
            this.BackUpFolderPathVM.LoadedValue = PathUtil.GetSmartPath(App.GetCurrentDir(), config.TargetFolderPath);
            this.IfBackUpAtMinimizingVM.LoadedValue = config.ExecuteAtMinimizing;
            this.BackUpIntervalAtMinimizingVM.LoadedValue = config.IntervalMinAtMinimizing;
            this.IfNotifyBackUpAtMinimizingVM.LoadedValue = config.NotifyAtMinimizing;
            this.IfBackUpAtClosingVM.LoadedValue = config.ExecuteAtClosing;
            this.BackUpConditionVM.LoadedValue = config.Condition;
        }

        public override void AddEventHandlers()
        {
            // NOP
        }

        /// <summary>
        /// DB設定を保存する
        /// </summary>
        private void Save()
        {
            using FuncLog funcLog = new();

            switch (this.DBKindVM.InputedValue) {
                case DBKind.SQLite:
                    // SQLite
                    _ = this.SQLiteSettingVM.Save();
                    break;
                case DBKind.PostgreSQL:
                    // PostgreSQL
                    _ = this.PostgreSQLDBSettingVM.Save(null);
                    break;
            }
            // Access(記帳風月)
            _ = this.AccessSettingVM.SaveForKichoFugetsu();

            // バックアップ
            UserSettingService.Instance.DbBackupConfig = new() {
                Amount = this.BackUpNumVM.InputedValue,
                TargetFolderPath = Path.GetFullPath(this.BackUpFolderPathVM.InputedValue, App.GetCurrentDir()),
                ExecuteAtMinimizing = this.IfBackUpAtMinimizingVM.InputedValue,
                IntervalMinAtMinimizing = this.BackUpIntervalAtMinimizingVM.InputedValue,
                NotifyAtMinimizing = this.IfNotifyBackUpAtMinimizingVM.InputedValue,
                ExecuteAtClosing = this.IfBackUpAtClosingVM.InputedValue,
                Condition = this.BackUpConditionVM.InputedValue
            };

            // SQLiteDbHandlerへ設定を反映する
            SQLiteDbHandler.Config = UserSettingService.Instance.SQLiteConfig;

            // DbBackUpManagerへ設定を反映する
            DbBackUpService.Instance.Config = UserSettingService.Instance.DbBackupConfig;
            DbBackUpService.Instance.NpgsqlConfig = UserSettingService.Instance.PostgreSQLBackupConfig;
        }
    }
}
