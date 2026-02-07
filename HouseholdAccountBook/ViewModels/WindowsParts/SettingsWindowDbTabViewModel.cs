using HouseholdAccountBook.Adapters;
using HouseholdAccountBook.Adapters.Logger;
using HouseholdAccountBook.Args.RequestEventArgs;
using HouseholdAccountBook.Enums;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.Utilities;
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
        #region SelectedDBKind
        public DBKind SelectedDBKind {
            get;
            set => this.SetProperty(ref field, value);
        }
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
        public FileDbSettingViewModel SQLiteSettingVM {
            get;
            set => this.SetProperty(ref field, value);
        } = new();
        #endregion
        #endregion

        #region バックアップ
        /// <summary>
        /// バックアップ数
        /// </summary>
        #region BackUpNum
        public int BackUpNum {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// バックアップ先フォルダ
        /// </summary>
        #region BackUpFolderPath
        public string BackUpFolderPath {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// メインウィンドウ最小化時バックアップフラグ
        /// </summary>
        #region BackUpFlagAtMinimizing
        public bool BackUpFlagAtMinimizing {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// メインウィンドウ最小化時バックアップインターバル(分)
        /// </summary>
        #region BackUpIntervalAtMinimizing
        public int BackUpIntervalAtMinimizing {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// メインウィンドウクローズ時バックアップフラグ
        /// </summary>
        #region BackUpFlagAtClosing
        public bool BackUpFlagAtClosing {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        #endregion

        #region コマンド
        /// <summary>
        /// ファイル選択コマンド
        /// </summary>
        public override ICommand SelectFilePathCommand => new RelayCommand<FilePathKind>(this.SelectFilePathCommand_Executed);
        /// <summary>
        /// データベース再設定コマンド
        /// </summary>
        public ICommand RestartForDbSettingCommand => new RelayCommand(this.RestartForDbSettingCommand_Executed);
        /// <summary>
        /// フォルダ選択コマンド
        /// </summary>
        public override ICommand SelectFolderPathCommand => new RelayCommand<FolderPathKind>(this.SelectFolderPathCommand_Executed);
        /// <summary>
        /// DB設定保存コマンド
        /// </summary>
        public ICommand SaveDbSettingsCommand => new RelayCommand(this.SaveDbSettingsCommand_Executed);
        #endregion
        #endregion

        #region コマンドイベントハンドラ
        /// <summary>
        /// ファイル選択コマンド処理
        /// </summary>
        /// <param name="kind">ファイルパス種別</param>
        private void SelectFilePathCommand_Executed(FilePathKind kind)
        {
            bool checkFileExists = true;
            string directory = string.Empty;
            string fileName = string.Empty;
            string filter = string.Empty;
            switch (kind) {
                case FilePathKind.DumpExeFile: {
                    (directory, fileName) = PathExtensions.GetSeparatedPath(this.PostgreSQLDBSettingVM.DumpExePath, App.GetCurrentDir());
                    if (string.IsNullOrWhiteSpace(directory)) {
                        directory = App.GetCurrentDir();
                    }
                    filter = "pg_dump.exe|pg_dump.exe";
                    break;
                }
                case FilePathKind.RestoreExeFile: {
                    (directory, fileName) = PathExtensions.GetSeparatedPath(this.PostgreSQLDBSettingVM.RestoreExePath, App.GetCurrentDir());
                    if (string.IsNullOrWhiteSpace(directory)) {
                        directory = App.GetCurrentDir();
                    }
                    filter = "pg_restore.exe|pg_restore.exe";
                    break;
                }
                default:
                    break;
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
                        this.PostgreSQLDBSettingVM.DumpExePath = PathExtensions.GetSmartPath(App.GetCurrentDir(), e.FileName);
                        break;
                    case FilePathKind.RestoreExeFile:
                        this.PostgreSQLDBSettingVM.RestoreExePath = PathExtensions.GetSmartPath(App.GetCurrentDir(), e.FileName);
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// データベース設定コマンド処理
        /// </summary>
        private void RestartForDbSettingCommand_Executed()
        {
            if (MessageBox.Show(Properties.Resources.Message_RestartNotification, Properties.Resources.Title_Conformation,
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK) {
                Properties.Settings.Default.App_InitFlag = true;
                Properties.Settings.Default.Save();

                ((App)Application.Current).Restart();
            }
        }

        /// <summary>
        /// フォルダ選択コマンド処理
        /// </summary>
        private void SelectFolderPathCommand_Executed(FolderPathKind kind)
        {
            string folderFullPath = string.Empty;
            string title = string.Empty;
            switch (kind) {
                case FolderPathKind.BackUpFolder:
                    if (string.IsNullOrWhiteSpace(this.BackUpFolderPath)) {
                        folderFullPath = App.GetCurrentDir();
                    }
                    else {
                        (string folderPath, string fileName) = PathExtensions.GetSeparatedPath(this.BackUpFolderPath, App.GetCurrentDir());
                        folderFullPath = Path.Combine(folderPath, fileName);
                    }
                    title = Properties.Resources.Title_BackupFolderSelection;
                    break;
                default:
                    break;
            }

            OpenFolderDialogRequestEventArgs e = new() {
                InitialDirectory = folderFullPath,
                Title = title
            };
            if (this.OpenFolderDialogRequest(e)) {
                switch (kind) {
                    case FolderPathKind.BackUpFolder:
                        this.BackUpFolderPath = PathExtensions.GetSmartPath(App.GetCurrentDir(), e.FolderName);
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// DB設定保存コマンド処理
        /// </summary>
        private void SaveDbSettingsCommand_Executed()
        {
            this.Save();
            this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        public override Task LoadAsync() => throw new NotImplementedException();

        /// <summary>
        /// DB設定を読み込む
        /// </summary>
        public void Load()
        {
            using FuncLog funcLog = new();

            Properties.Settings settings = Properties.Settings.Default;

            this.SelectedDBKind = (DBKind)settings.App_SelectedDBKind;

            // PostgreSQL
            this.PostgreSQLDBSettingVM.Load(null);
            // SQLite
            this.SQLiteSettingVM.Load();
            // Access(記帳風月)
            this.AccessSettingVM.LoadForKichoFugetsu();

            // バックアップ
            this.BackUpNum = settings.App_BackUpNum;
            this.BackUpFolderPath = PathExtensions.GetSmartPath(App.GetCurrentDir(), settings.App_BackUpFolderPath);
            this.BackUpFlagAtMinimizing = settings.App_BackUpFlagAtMinimizing;
            this.BackUpIntervalAtMinimizing = settings.App_BackUpIntervalMinAtMinimizing;
            this.BackUpFlagAtClosing = settings.App_BackUpFlagAtClosing;
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

            Properties.Settings settings = Properties.Settings.Default;

            // PostgreSQL
            _ = this.PostgreSQLDBSettingVM.Save(null);
            // Access(記帳風月)
            _ = this.AccessSettingVM.SaveForKichoFugetsu();

            // バックアップ
            settings.App_BackUpNum = this.BackUpNum;
            settings.App_BackUpFolderPath = Path.GetFullPath(this.BackUpFolderPath, App.GetCurrentDir());
            settings.App_BackUpFlagAtMinimizing = this.BackUpFlagAtMinimizing;
            settings.App_BackUpIntervalMinAtMinimizing = this.BackUpIntervalAtMinimizing;
            settings.App_BackUpFlagAtClosing = this.BackUpFlagAtClosing;

            // DbBackUpManagerへ設定を反映する
            DbBackUpManager.Instance.PostgresDumpExePath = settings.App_Postgres_DumpExePath;
            DbBackUpManager.Instance.PostgresRestoreExePath = settings.App_Postgres_RestoreExePath;
            DbBackUpManager.Instance.PostgresPasswordInput = (PostgresPasswordInput)settings.App_Postgres_Password_Input;

            DbBackUpManager.Instance.BackUpNum = settings.App_BackUpNum;
            DbBackUpManager.Instance.BackUpFolderPath = settings.App_BackUpFolderPath;
            DbBackUpManager.Instance.BackUpFlagAtMinimizing = settings.App_BackUpFlagAtMinimizing;
            DbBackUpManager.Instance.BackUpIntervalMinAtMinimizing = settings.App_BackUpIntervalMinAtMinimizing;
            DbBackUpManager.Instance.BackUpFlagAtClosing = settings.App_BackUpFlagAtClosing;

            settings.Save();
        }
    }
}
