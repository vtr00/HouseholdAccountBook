using HouseholdAccountBook.Infrastructure.DB;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.Utilities;
using HouseholdAccountBook.Infrastructure.Utilities.Args.RequestEventArgs;
using HouseholdAccountBook.Models.AppServices;
using HouseholdAccountBook.ViewModels.Abstract;
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
        /// 選択されたDB種別
        /// </summary>
        public DBKind SelectedDBKind {
            get;
            set => this.SetProperty(ref field, value);
        }

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
        public SQLiteSettingViewModel SQLiteSettingVM { get; } = new();
        #endregion

        #region バックアップ
        /// <summary>
        /// 入力されたバックアップ数
        /// </summary>
        public int InputedBackUpNum {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// 入力されたバックアップ先フォルダ
        /// </summary>
        public string InputedBackUpFolderPath {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// 選択されたメインウィンドウ最小化時バックアップするか
        /// </summary>
        public bool SelectedIfBackUpAtMinimizing {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// 入力されたメインウィンドウ最小化時バックアップインターバル(分)
        /// </summary>
        public int InputedBackUpIntervalAtMinimizing {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// 選択されたメインウィンドウクローズ時バックアップするか
        /// </summary>
        public bool SelectedIfBackUpAtClosing {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// 選択されたバックアップ条件
        /// </summary>
        public BackUpCondition SelectedBackUpCondition {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// 選択されたメインウィンドウ最小化時バックアップを通知するか
        /// </summary>
        public bool SelectedIfNotifyBackUpAtMinimizing {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

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
        /// データベース再設定コマンド
        /// </summary>
        public ICommand RestartForDbSettingCommand => field ??= new RelayCommand(this.RestartForDbSettingCommand_Execute);
        /// <summary>
        /// CSVフォルダ選択コマンド
        /// </summary>
        public ICommand SelectCsvFolderPathCommand => field ??= new RelayCommand(this.SelectCsvFolderPathCommand_Execute);
        /// <summary>
        /// DB設定保存コマンド
        /// </summary>
        public ICommand SaveDbSettingsCommand => field ??= new RelayCommand(this.SaveDbSettingsCommand_Execute);
        #endregion
        #endregion

        #region コマンドイベントハンドラ
        /// <summary>
        /// ファイル選択コマンド処理
        /// </summary>
        private void SelectPgDumpFilePathCommand_Execute()
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
        /// ファイル選択コマンド処理
        /// </summary>
        private void SelectPgRestoreFilePathCommand_Execute()
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
        /// CSVフォルダ選択コマンド処理
        /// </summary>
        private void SelectCsvFolderPathCommand_Execute()
        {
            string folderFullPath;
            if (string.IsNullOrWhiteSpace(this.InputedBackUpFolderPath)) {
                folderFullPath = App.GetCurrentDir();
            }
            else {
                (string folderPath, string fileName) = PathUtil.GetSeparatedPath(this.InputedBackUpFolderPath, App.GetCurrentDir());
                folderFullPath = Path.Combine(folderPath, fileName);
            }

            OpenFolderDialogRequestEventArgs e = new() {
                InitialDirectory = folderFullPath,
                Title = Properties.Resources.Title_BackupFolderSelection
            };
            if (this.OpenFolderDialogRequest(e)) {
                this.InputedBackUpFolderPath = PathUtil.GetSmartPath(App.GetCurrentDir(), e.FolderName);
            }
        }

        /// <summary>
        /// DB設定保存コマンド処理
        /// </summary>
        private void SaveDbSettingsCommand_Execute()
        {
            this.Save();
            this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        /// <summary>
        /// DB設定を読み込む
        /// </summary>
        public override async Task LoadAsync()
        {
            using FuncLog funcLog = new();

            this.SelectedDBKind = UserSettingService.Instance.SelectedDBKind;

            // PostgreSQL
            this.PostgreSQLDBSettingVM.Load(null);
            // SQLite
            this.SQLiteSettingVM.Load();
            // Access(記帳風月)
            await this.AccessSettingVM.LoadForKichoFugetsuAsync();

            // バックアップ
            DbBackUpManager.Configuration config = UserSettingService.Instance.DbBackupConfig;
            this.InputedBackUpNum = config.Amount;
            this.InputedBackUpFolderPath = PathUtil.GetSmartPath(App.GetCurrentDir(), config.TargetFolderPath);
            this.SelectedIfBackUpAtMinimizing = config.ExecuteAtMinimizing;
            this.InputedBackUpIntervalAtMinimizing = config.IntervalMinAtMinimizing;
            this.SelectedIfNotifyBackUpAtMinimizing = config.NotifyAtMinimizing;
            this.SelectedIfBackUpAtClosing = config.ExecuteAtClosing;
            this.SelectedBackUpCondition = config.Condition;
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

            // PostgreSQL
            _ = this.PostgreSQLDBSettingVM.Save(null);
            // Access(記帳風月)
            _ = this.AccessSettingVM.SaveForKichoFugetsu();

            // バックアップ
            UserSettingService.Instance.DbBackupConfig = new() {
                Amount = this.InputedBackUpNum,
                TargetFolderPath = Path.GetFullPath(this.InputedBackUpFolderPath, App.GetCurrentDir()),
                ExecuteAtMinimizing = this.SelectedIfBackUpAtMinimizing,
                IntervalMinAtMinimizing = this.InputedBackUpIntervalAtMinimizing,
                NotifyAtMinimizing = this.SelectedIfNotifyBackUpAtMinimizing,
                ExecuteAtClosing = this.SelectedIfBackUpAtClosing,
                Condition = this.SelectedBackUpCondition
            };

            // DbBackUpManagerへ設定を反映する
            DbBackUpManager.Instance.Config = UserSettingService.Instance.DbBackupConfig;

            DbBackUpManager.Instance.NpgsqlBackupConfig = UserSettingService.Instance.PostgreSQLBackupConfig;
        }
    }
}
