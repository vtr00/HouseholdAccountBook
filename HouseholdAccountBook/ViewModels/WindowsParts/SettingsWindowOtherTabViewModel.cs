using HouseholdAccountBook.Infrastructure.CSV;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.Services;
using HouseholdAccountBook.Infrastructure.Utilities;
using HouseholdAccountBook.Models.AppServices;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Common;
using HouseholdAccountBook.ViewModels.Settings;
using HouseholdAccountBook.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
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
        #region 言語
        /// <summary>
        /// 言語種別リストVM
        /// </summary>
        public SelectableViewModel<KeyValuePair<string, string>, string> CultureNameListVM { get; } = new(static p => p.Key);

        /// <summary>
        /// 言語未保存状態マネージャ
        /// </summary>
        public DirtyStatusManager LanguageDirtyState => field ??= new(this.CultureNameListVM);
        #endregion

        #region バージョン
        /// <summary>
        /// アプリ起動時最新バージョン通知VM
        /// </summary>
        public InputableViewModel<bool> IfNotifyLatestAtAppLaunchedVM { get; } = new();

        /// <summary>
        /// バージョン未保存状態マネージャ
        /// </summary>
        public DirtyStatusManager AppVersionDirtyState => field ??= new(this.IfNotifyLatestAtAppLaunchedVM);
        #endregion

        #region カレンダー
        /// <summary>
        /// 開始月VM
        /// </summary>
        public InputableViewModel<int> StartMonthVM { get; } = new();

        /// <summary>
        /// 国民の祝日CSV URI VM
        /// </summary>
        public InputableViewModel<string> NationalHolidayCsvURIVM { get; } = new();

        /// <summary>
        /// 国民の祝日CSV 文字エンコーディング リストVM
        /// </summary>
        public SelectableViewModel<KeyValuePair<int, string>, int> NationalHolidayTextEncodingListVM => field ??= new(static p => p.Key);

        /// <summary>
        /// 国民の祝日CSV 日付インデックス(1開始) VM
        /// </summary>
        public InputableViewModel<int> NationalHolidayCsvDateIndexVM { get; } = new();

        /// <summary>
        /// カレンダー未保存状態マネージャ
        /// </summary>
        public DirtyStatusManager CalendarDirtyStatus => field ??= new(this.StartMonthVM, this.NationalHolidayCsvURIVM, this.NationalHolidayTextEncodingListVM, this.NationalHolidayCsvDateIndexVM);
        #endregion

        #region ウィンドウ
        /// <summary>
        /// ウィンドウ位置保存VM
        /// </summary>
        public InputableViewModel<bool> IsPositionSavedVM { get; } = new();

        /// <summary>
        /// ウィンドウ設定
        /// </summary>
        public ObservableCollection<WindowSettingViewModel> WindowSettingVMList {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// ウィンドウ未保存状態マネージャ
        /// </summary>
        public DirtyStatusManager WindowDirtyState => field ??= new(this.IsPositionSavedVM);
        #endregion

        #region ログ
        /// <summary>
        /// 操作ログ出力VM
        /// </summary>
        public InputableViewModel<bool> IfOutputOperationLogVM { get; } = new();
        /// <summary>
        /// 操作ログ数VM
        /// </summary>
        public InputableViewModel<int> OperationLogNumVM { get; } = new();
        /// <summary>
        /// ログレベル リストVM
        /// </summary>
        public SelectableViewModel<KeyValuePair<Log.LogLevel, string>, Log.LogLevel> LogLevelListVM => field ??= new(static p => p.Key);

        /// <summary>
        /// ウィンドウログ出力VM
        /// </summary>
        public InputableViewModel<bool> IfOutputWindowLogVM { get; } = new();
        /// <summary>
        /// ウィンドウログ数VM
        /// </summary>
        public InputableViewModel<int> WindowLogNumVM { get; } = new();

        /// <summary>
        /// 捕捉されない例外ログ数VM
        /// </summary>
        public InputableViewModel<int> UnhandledExceptionLogNumVM { get; } = new();

        /// <summary>
        /// ログ未保存状態マネージャ
        /// </summary>
        public DirtyStatusManager LogDirtyState => field ??=
            new(this.IfOutputOperationLogVM, this.OperationLogNumVM, this.LogLevelListVM, this.IfOutputWindowLogVM, this.WindowLogNumVM, this.UnhandledExceptionLogNumVM);
        #endregion

        /// <summary>
        /// 未保存状態マネージャ
        /// </summary>
        public DirtyStatusManager DirtyStatus => field ??= new(this.AppVersionDirtyState, this.CalendarDirtyStatus, this.WindowDirtyState, this.LogDirtyState);
        #endregion

        #region コマンド
        /// <summary>
        /// 言語設定適用コマンド
        /// </summary>
        public ICommand RestartForLanguageCommand => field ??= new RelayCommand(this.RestartForLanguageCommand_Execute, () => this.LanguageDirtyState.IsDirty);
        /// <summary>
        /// 言語設定適用コマンド処理
        /// </summary>
        private void RestartForLanguageCommand_Execute()
        {
            if (MessageBox.Show(Properties.Resources.Message_RestartNotification, Properties.Resources.Title_Conformation,
                                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK) {
                UserSettingService.Instance.SelectedCaltureName = this.CultureNameListVM.SelectedKey;

                ((App)Application.Current).Restart();
            }
        }

        /// <summary>
        /// ウィンドウ設定再読込コマンド
        /// </summary>
        public ICommand ReloadWindowSettingCommand => field ??= new RelayCommand(() => this.WindowSettingVMList = LoadWindowSettings());

        /// <summary>
        /// ウィンドウ設定初期化コマンド
        /// </summary>
        public ICommand InitializeWindowSettingCommand => field ??= new RelayCommand(this.InitializeWindowSettingCommand_Execute);
        /// <summary>
        /// ウィンドウ設定初期化コマンド処理
        /// </summary>
        private void InitializeWindowSettingCommand_Execute()
        {
            if (MessageBox.Show(Properties.Resources.Message_RestartNotification, Properties.Resources.Title_Conformation,
                                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK) {
                UserSettingService.Instance.InitializeWindowLocation();

                ((App)Application.Current).Restart(false);
            }
        }

        /// <summary>
        /// その他設定保存コマンド
        /// </summary>
        public ICommand SaveOtherSettingsCommand => field ??= new AsyncRelayCommand(this.SaveOtherSettingsCommand_Execute, () => this.DirtyStatus.IsDirty);
        /// <summary>
        /// その他設定保存コマンド処理
        /// </summary>
        private async Task SaveOtherSettingsCommand_Execute()
        {
            this.Save();
            // 再読込する
            await this.LoadOtherAsync();

            _ = MessageBox.Show(Properties.Resources.Message_CompletedToSave, Properties.Resources.Title_Information, MessageBoxButton.OK, MessageBoxImage.Information);
            this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SettingsWindowOtherTabViewModel(BusyService busyService) : base(busyService)
        {
            using FuncLog funcLog = new();
        }

        public override void Initialize(DbHandlerFactory dbHandlerFactory)
        {
            using FuncLog funcLog = new();

            base.Initialize(dbHandlerFactory);

            this.CultureNameListVM.SetLoader(static () => UiConstants.CultureNameStr);
            this.NationalHolidayTextEncodingListVM.SetLoader(static () => EncodingUtil.GetTextEncodingList());
            this.LogLevelListVM.SetLoader(static () => {
                Dictionary<Log.LogLevel, string> dic = UiConstants.LogLevelStr;
#if !DEBUG
                _ = dic.Remove(Log.LogLevel.Trace);
#endif
                return dic;
            });
        }

        /// <summary>
        /// 言語情報を読み込む
        /// </summary>
        /// <returns></returns>
        private async Task LoadLanguageAsync()
        {
            using FuncLog funcLog = new();

            // 言語情報
            await this.CultureNameListVM.LoadAsync(UserSettingService.Instance.SelectedCaltureName);
        }

        /// <summary>
        /// その他設定(言語情報を除く)を読み込む
        /// </summary>
        /// <returns></returns>
        private async Task LoadOtherAsync()
        {
            using FuncLog funcLog = new();

            // バージョン情報
            this.IfNotifyLatestAtAppLaunchedVM.LoadedValue = UserSettingService.Instance.CheckLatestVersionAtAppLaunched;

            // カレンダー情報
            this.StartMonthVM.LoadedValue = UserSettingService.Instance.FiscalStartMonth;
            CSVFileDao.HolidayCSVConfigulation holidayCsvConfig = UserSettingService.Instance.HolidayCSVConfig;
            this.NationalHolidayCsvURIVM.LoadedValue = holidayCsvConfig.Url;
            await this.NationalHolidayTextEncodingListVM.LoadAsync(holidayCsvConfig.TextEncoding.CodePage);
            this.NationalHolidayCsvDateIndexVM.LoadedValue = holidayCsvConfig.DateIndex + 1;

            // ウィンドウ情報
            WindowLocationManager.Configuration locationConfig = UserSettingService.Instance.WindowLocationConfig;
            this.IsPositionSavedVM.LoadedValue = locationConfig.IsPositionSaved;
            this.WindowSettingVMList = LoadWindowSettings();

            // ログ情報
            LogImpl.Configuration logConfig = UserSettingService.Instance.LogConfig;
            this.IfOutputOperationLogVM.LoadedValue = logConfig.OutputLogToFile;
            this.OperationLogNumVM.LoadedValue = logConfig.LogFileAmount;
            await this.LogLevelListVM.LoadAsync(logConfig.OutputLogLevel);
            WindowLog.Configulation windowLogConfig = UserSettingService.Instance.WindowLogConfig;
            this.IfOutputWindowLogVM.LoadedValue = windowLogConfig.OutputLog;
            this.WindowLogNumVM.LoadedValue = windowLogConfig.LogFileAmount;
            ExceptionLog.Configuration exceptionLogConfig = UserSettingService.Instance.ExceptionLogConfig;
            this.UnhandledExceptionLogNumVM.LoadedValue = exceptionLogConfig.LogFileAmount;
        }

        /// <summary>
        /// その他設定を読み込む
        /// </summary>
        public override async Task LoadAsync()
        {
            using FuncLog funcLog = new();

            await this.LoadLanguageAsync();
            await this.LoadOtherAsync();
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

            ObservableCollection<WindowSettingViewModel> list = [
                new WindowSettingViewModel(){
                    Title = Properties.Resources.Title_DbSettingWindow,
                    Point = UserSettingService.Instance.DbSettingWindowPoint,
                    Size = UserSettingService.Instance.DbSettingWindowSize
                },
                new WindowSettingViewModel(){
                    Title = Properties.Resources.Title_MainWindow,
                    Point = UserSettingService.Instance.MainWindowPoint,
                    Size = UserSettingService.Instance.MainWindowSize
                },
                new WindowSettingViewModel(){
                    Title = Properties.Resources.Title_AddMoveWindow,
                    Point = UserSettingService.Instance.MoveRegistrationWindowPoint,
                    Size = UserSettingService.Instance.MoveRegistrationWindowSize
                },
                new WindowSettingViewModel(){
                    Title = Properties.Resources.Title_AddExchangeWindow,
                    Point = UserSettingService.Instance.ExchangeRegistrationWindowPoint,
                    Size = UserSettingService.Instance.ExchangeRegistrationWindowSize
                },
                new WindowSettingViewModel(){
                    Title = Properties.Resources.Title_AddWindow,
                    Point = UserSettingService.Instance.ActionRegistrationWindowPoint,
                    Size = UserSettingService.Instance.ActionRegistrationWindowSize
                },
                new WindowSettingViewModel(){
                    Title = Properties.Resources.Title_AddListWindow,
                    Point = UserSettingService.Instance.ActionListRegistrationWindowPoint,
                    Size = UserSettingService.Instance.ActionListRegistrationWindowSize
                },
                new WindowSettingViewModel(){
                    Title = Properties.Resources.Title_CsvComparisonWindow,
                    Point = UserSettingService.Instance.CsvComparisonWindowPoint,
                    Size = UserSettingService.Instance.CsvComparisonWindowSize
                },
                new WindowSettingViewModel(){
                    Title = Properties.Resources.Title_SettingsWindow,
                    Point = UserSettingService.Instance.SettingsWindowPoint,
                    Size = UserSettingService.Instance.SettingsWindowSize
                },
                new WindowSettingViewModel(){
                    Title = Properties.Resources.Title_PeriodSelectionWindow,
                    Point = UserSettingService.Instance.TermWindowPoint,
                    Size = (-1, -1)
                },
                new WindowSettingViewModel(){
                    Title = Properties.Resources.Title_VersionWindow,
                    Point = UserSettingService.Instance.VersionWindowPoint,
                    Size = UserSettingService.Instance.VersionWindowSize
                }
            ];
            return list;
        }

        /// <summary>
        /// その他設定を保存する
        /// </summary>
        /// <remarks>言語情報はここでは保存しない</remarks>
        private void Save()
        {
            using FuncLog funcLog = new();

            // バージョン情報
            UserSettingService.Instance.CheckLatestVersionAtAppLaunched = this.IfNotifyLatestAtAppLaunchedVM.InputedValue;

            // カレンダー情報
            UserSettingService.Instance.FiscalStartMonth = this.StartMonthVM.InputedValue;
            UserSettingService.Instance.HolidayCSVConfig = new() {
                Url = this.NationalHolidayCsvURIVM.InputedValue,
                TextEncoding = Encoding.GetEncoding(this.NationalHolidayTextEncodingListVM.SelectedKey),
                DateIndex = this.NationalHolidayCsvDateIndexVM.InputedValue - 1
            };

            // ウィンドウ情報
            UserSettingService.Instance.WindowLocationConfig = new() {
                IsPositionSaved = this.IsPositionSavedVM.InputedValue
            };

            // ログ情報
            UserSettingService.Instance.LogConfig = new() {
                OutputLogToFile = this.IfOutputOperationLogVM.InputedValue,
                LogFileAmount = this.OperationLogNumVM.InputedValue,
                OutputLogLevel = this.LogLevelListVM.SelectedKey
            };
            UserSettingService.Instance.WindowLogConfig = new() {
                OutputLog = this.IfOutputWindowLogVM.InputedValue,
                LogFileAmount = this.WindowLogNumVM.InputedValue
            };
            UserSettingService.Instance.ExceptionLogConfig = new() {
                LogFileAmount = this.UnhandledExceptionLogNumVM.InputedValue
            };

            // 設定をログクラスに反映する
            LogImpl.Instance.Config = UserSettingService.Instance.LogConfig;
            ExceptionLog.Config = UserSettingService.Instance.ExceptionLogConfig;
            WindowLog.Config = UserSettingService.Instance.WindowLogConfig;

            // 新しい設定に合わせて古いログファイルを削除する
            LogImpl.DeleteOldLogFiles();
            ExceptionLog.DeleteOldExceptionLogs();
            WindowLog.DeleteAllOldWindowLogs([.. UiConstants.WindowNameStr.Values]);
        }
    }
}
