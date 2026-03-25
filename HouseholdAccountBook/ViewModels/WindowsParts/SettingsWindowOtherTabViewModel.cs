using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.Utilities;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HouseholdAccountBook.ViewModels.WindowsParts
{
    /// <summary>
    /// その他設定タブVM
    /// </summary>
    public class SettingsWindowOtherTabViewModel : WindowPartViewModelBase
    {
        #region イベント
        /// <summary>
        /// 更新必要時イベント
        /// </summary>
        public event EventHandler NeedToUpdateChanged;
        #endregion

        #region Bindingプロパティ
        #region バージョン
        /// <summary>
        /// 入力されたアプリ起動時最新バージョン通知
        /// </summary>
        public bool SelectedIfNotifyLatestAtAppLaunched {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        #region 言語
        /// <summary>
        /// 言語種別セレクタVM
        /// </summary>
        public SelectorViewModel<KeyValuePair<string, string>, string> CultureNameSelectorVM { get; } = new(static p => p.Key);
        #endregion

        #region カレンダー
        /// <summary>
        /// 入力された開始月
        /// </summary>
        public int InputedStartMonth {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// 入力された国民の祝日CSV URI
        /// </summary>
        public string InputedNationalHolidayCsvURI {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// 国民の祝日CSV 文字エンコーディング セレクタVM
        /// </summary>
        public SelectorViewModel<KeyValuePair<int, string>, int> NationalHolidayTextEncodingSelectorVM { get; } = new(static p => p.Key);

        /// <summary>
        /// 入力された国民の祝日CSV 日付インデックス(1開始)
        /// </summary>
        public int InputedNationalHolidayCsvDateIndex {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        #region ウィンドウ
        /// <summary>
        /// ウィンドウ位置を保存するか
        /// </summary>
        public bool IsPositionSaved {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// ウィンドウ設定
        /// </summary>
        public ObservableCollection<WindowSettingViewModel> WindowSettingVMList {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        #region ログ
        /// <summary>
        /// 選択された操作ログ出力の有無
        /// </summary>
        public bool SelectedIfOutputOperationLog {
            get;
            set => this.SetProperty(ref field, value);
        }
        /// <summary>
        /// 入力された操作ログ数
        /// </summary>
        public int InputedOperationLogNum {
            get;
            set => this.SetProperty(ref field, value);
        }
        /// <summary>
        /// ログレベル セレクタVM
        /// </summary>
        public SelectorViewModel<KeyValuePair<Log.LogLevel, string>, Log.LogLevel> LogLevelSelectorVM { get; } = new(static p => p.Key);

        /// <summary>
        /// 選択されたウィンドウログ出力の有無
        /// </summary>
        public bool SelectedIfOutputWindowLog {
            get;
            set => this.SetProperty(ref field, value);
        }
        /// <summary>
        /// 入力されたウィンドウログ数
        /// </summary>
        public int InputedWindowLogNum {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// 入力された捕捉されない例外ログ数
        /// </summary>
        public int InputedUnhandledExceptionLogNum {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        #region コマンド
        /// <summary>
        /// 言語設定適用コマンド
        /// </summary>
        public ICommand RestartForLanguageCommand => new RelayCommand(this.RestartForLanguageCommand_Execute);
        /// <summary>
        /// ウィンドウ設定再読込コマンド
        /// </summary>
        public ICommand ReloadWindowSettingCommand => new RelayCommand(this.ReloadWindowSettingCommand_Execute);
        /// <summary>
        /// ウィンドウ設定初期化コマンド
        /// </summary>
        public ICommand InitializeWindowSettingCommand => new RelayCommand(this.InitializeWindowSettingCommand_Execute);
        /// <summary>
        /// その他設定保存コマンド
        /// </summary>
        public ICommand SaveOtherSettingsCommand => new RelayCommand(this.SaveOtherSettingsCommand_Execute);
        #endregion
        #endregion

        #region コマンドイベントハンドラ
        /// <summary>
        /// 言語設定適用コマンド処理
        /// </summary>
        private void RestartForLanguageCommand_Execute()
        {
            if (MessageBox.Show(Properties.Resources.Message_RestartNotification, Properties.Resources.Title_Conformation,
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK) {
                Properties.Settings.Default.App_CultureName = this.CultureNameSelectorVM.SelectedKey;
                Properties.Settings.Default.Save();

                ((App)Application.Current).Restart();
            }
        }

        /// <summary>
        /// ウィンドウ設定再読込コマンド処理
        /// </summary>
        private void ReloadWindowSettingCommand_Execute() => this.WindowSettingVMList = LoadWindowSettings();

        /// <summary>
        /// ウィンドウ設定初期化コマンド処理
        /// </summary>
        private void InitializeWindowSettingCommand_Execute()
        {
            if (MessageBox.Show(Properties.Resources.Message_RestartNotification, Properties.Resources.Title_Conformation,
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK) {
                Properties.Settings settings = Properties.Settings.Default;

                // DB設定
                settings.DbSettingWindow_Left = -1;
                settings.DbSettingWindow_Top = -1;
                settings.DbSettingWindow_Width = -1;
                settings.DbSettingWindow_Height = -1;

                // メイン
                settings.MainWindow_Left = -1;
                settings.MainWindow_Top = -1;
                settings.MainWindow_Width = -1;
                settings.MainWindow_Height = -1;

                // 移動
                settings.MoveRegistrationWindow_Left = -1;
                settings.MoveRegistrationWindow_Top = -1;
                settings.MoveRegistrationWindow_Width = -1;
                settings.MoveRegistrationWindow_Height = -1;

                // 追加・変更
                settings.ActionRegistrationWindow_Left = -1;
                settings.ActionRegistrationWindow_Top = -1;
                settings.ActionRegistrationWindow_Width = -1;
                settings.ActionRegistrationWindow_Height = -1;

                // リスト追加
                settings.ActionListRegistrationWindow_Left = -1;
                settings.ActionListRegistrationWindow_Top = -1;
                settings.ActionListRegistrationWindow_Width = -1;
                settings.ActionListRegistrationWindow_Height = -1;

                // CSV比較
                settings.CsvComparisonWindow_Left = -1;
                settings.CsvComparisonWindow_Top = -1;
                settings.CsvComparisonWindow_Width = -1;
                settings.CsvComparisonWindow_Height = -1;

                // 設定
                settings.SettingsWindow_Left = -1;
                settings.SettingsWindow_Top = -1;
                settings.SettingsWindow_Width = -1;
                settings.SettingsWindow_Height = -1;

                // 期間選択
                settings.TermWindow_Left = -1;
                settings.TermWindow_Top = -1;
                // settings.TermWindow_Width = -1;
                // settings.TermWindow_Height = -1;

                // バージョン
                settings.VersionWindow_Left = -1;
                settings.VersionWindow_Top = -1;
                settings.VersionWindow_Width = -1;
                settings.VersionWindow_Height = -1;

                settings.App_InitSizeFlag = true;
                settings.Save();

                ((App)Application.Current).Restart();
            }
        }

        /// <summary>
        /// その他設定保存コマンド処理
        /// </summary>
        private void SaveOtherSettingsCommand_Execute()
        {
            this.Save();
            this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SettingsWindowOtherTabViewModel()
        {
            using FuncLog funcLog = new();

            this.CultureNameSelectorVM.SetLoader(static () => UiConstants.CultureNameStr);
            this.NationalHolidayTextEncodingSelectorVM.SetLoader(static () => EncodingUtil.GetTextEncodingList());
            this.LogLevelSelectorVM.SetLoader(static () => {
                Dictionary<Log.LogLevel, string> dic = UiConstants.LogLevelStr;
#if !DEBUG
                _ = dic.Remove(Log.LogLevel.Trace);
#endif
                return dic;
            });
        }

        /// <summary>
        /// その他設定を読み込む
        /// </summary>
        public override async Task LoadAsync()
        {
            using FuncLog funcLog = new();

            Properties.Settings settings = Properties.Settings.Default;

            // バージョン情報
            this.SelectedIfNotifyLatestAtAppLaunched = settings.App_LatestVersionNotifyAtAppLaunched;

            // 言語情報
            await this.CultureNameSelectorVM.LoadAsync(settings.App_CultureName);

            // カレンダー情報
            this.InputedStartMonth = settings.App_StartMonth;
            this.InputedNationalHolidayCsvURI = settings.App_NationalHolidayCsv_Uri;
            await this.NationalHolidayTextEncodingSelectorVM.LoadAsync(settings.App_NationalHolidayCsv_TextEncoding);
            this.InputedNationalHolidayCsvDateIndex = settings.App_NationalHolidayCsv_DateIndex + 1;

            // ウィンドウ情報
            this.IsPositionSaved = settings.App_IsPositionSaved;
            this.WindowSettingVMList = LoadWindowSettings();

            // ログ情報
            this.SelectedIfOutputOperationLog = settings.App_OutputFlag_OperationLog;
            this.InputedOperationLogNum = settings.App_OperationLogNum;
            await this.LogLevelSelectorVM.LoadAsync((Log.LogLevel)settings.App_OperationLogLevel);
            this.SelectedIfOutputWindowLog = settings.App_OutputFlag_WindowLog;
            this.InputedWindowLogNum = settings.App_WindowLogNum;
            this.InputedUnhandledExceptionLogNum = settings.App_UnhandledExceptionLogNum;
        }

        public override void AddEventHandlers()
        {
            // NOP
        }

        /// <summary>
        /// 各ウィンドウの矩形領域設定を読み込む
        /// </summary>
        private static ObservableCollection<WindowSettingViewModel> LoadWindowSettings()
        {
            using FuncLog funcLog = new();

            Properties.Settings settings = Properties.Settings.Default;

            ObservableCollection<WindowSettingViewModel> list = [
                new WindowSettingViewModel(){
                    Title = Properties.Resources.Title_DbSettingWindow,
                    Left = settings.DbSettingWindow_Left, Top = settings.DbSettingWindow_Top,
                    Width = settings.DbSettingWindow_Width, Height = settings.DbSettingWindow_Height
                },
                new WindowSettingViewModel(){
                    Title = Properties.Resources.Title_MainWindow,
                    Left = settings.MainWindow_Left, Top = settings.MainWindow_Top,
                    Width = settings.MainWindow_Width, Height = settings.MainWindow_Height
                },
                new WindowSettingViewModel(){
                    Title = Properties.Resources.Title_AddMoveWindow,
                    Left = settings.MoveRegistrationWindow_Left, Top = settings.MoveRegistrationWindow_Top,
                    Width = settings.MoveRegistrationWindow_Width, Height = settings.MoveRegistrationWindow_Height
                },
                new WindowSettingViewModel(){
                    Title = Properties.Resources.Title_AddWindow,
                    Left = settings.ActionRegistrationWindow_Left, Top = settings.ActionRegistrationWindow_Top,
                    Width = settings.ActionRegistrationWindow_Width, Height = settings.ActionRegistrationWindow_Height
                },
                new WindowSettingViewModel(){
                    Title = Properties.Resources.Title_AddListWindow,
                    Left = settings.ActionListRegistrationWindow_Left, Top = settings.ActionListRegistrationWindow_Top,
                    Width = settings.ActionListRegistrationWindow_Width, Height = settings.ActionListRegistrationWindow_Height
                },
                new WindowSettingViewModel(){
                    Title = Properties.Resources.Title_CsvComparisonWindow,
                    Left = settings.CsvComparisonWindow_Left, Top = settings.CsvComparisonWindow_Top,
                    Width = settings.CsvComparisonWindow_Width, Height = settings.CsvComparisonWindow_Height
                },
                new WindowSettingViewModel(){
                    Title = Properties.Resources.Title_SettingsWindow,
                    Left = settings.SettingsWindow_Left, Top = settings.SettingsWindow_Top,
                    Width = settings.SettingsWindow_Width, Height = settings.SettingsWindow_Height
                },
                new WindowSettingViewModel(){
                    Title = Properties.Resources.Title_PeriodSelectionWindow,
                    Left = settings.TermWindow_Left, Top = settings.TermWindow_Top,
                    Width = -1, Height = -1
                },
                new WindowSettingViewModel(){
                    Title = Properties.Resources.Title_VersionWindow,
                    Left = settings.VersionWindow_Left, Top = settings.VersionWindow_Top,
                    Width = settings.VersionWindow_Width, Height = settings.VersionWindow_Height
                }
            ];
            return list;
        }

        /// <summary>
        /// その他設定を保存する
        /// </summary>
        private void Save()
        {
            using FuncLog funcLog = new();

            Properties.Settings settings = Properties.Settings.Default;

            // バージョン情報
            settings.App_LatestVersionNotifyAtAppLaunched = this.SelectedIfNotifyLatestAtAppLaunched;

            // 言語情報
            settings.App_CultureName = this.CultureNameSelectorVM.SelectedKey;

            // カレンダー情報
            settings.App_StartMonth = this.InputedStartMonth;
            settings.App_NationalHolidayCsv_Uri = this.InputedNationalHolidayCsvURI;
            settings.App_NationalHolidayCsv_TextEncoding = this.NationalHolidayTextEncodingSelectorVM.SelectedKey;
            settings.App_NationalHolidayCsv_DateIndex = this.InputedNationalHolidayCsvDateIndex - 1;

            // ウィンドウ情報
            settings.App_IsPositionSaved = this.IsPositionSaved;

            // ログ情報
            settings.App_OutputFlag_OperationLog = this.SelectedIfOutputOperationLog;
            settings.App_OperationLogNum = this.InputedOperationLogNum;
            settings.App_OperationLogLevel = (int)this.LogLevelSelectorVM.SelectedKey;
            settings.App_OutputFlag_WindowLog = this.SelectedIfOutputWindowLog;
            settings.App_WindowLogNum = this.InputedWindowLogNum;
            settings.App_UnhandledExceptionLogNum = this.InputedUnhandledExceptionLogNum;

            settings.Save();

            LogImpl.Instance.OutputLogToFile = settings.App_OutputFlag_OperationLog;
            LogImpl.Instance.LogFileAmount = settings.App_OperationLogNum;
            LogImpl.Instance.OutputLogLevel = (Log.LogLevel)settings.App_OperationLogLevel;
            ExceptionLog.LogFileAmount = settings.App_UnhandledExceptionLogNum;
            WindowLog.OutputLog = settings.App_OutputFlag_WindowLog;
            WindowLog.LogFileAmount = settings.App_WindowLogNum;

            // 新しい設定に合わせて古いログファイルを削除する
            LogImpl.DeleteOldLogFiles();
            ExceptionLog.DeleteOldExceptionLogs();
            WindowLog.DeleteAllOldWindowLogs([.. UiConstants.WindowNameStr.Values]);
        }
    }
}
