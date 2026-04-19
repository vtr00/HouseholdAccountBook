using HouseholdAccountBook.Infrastructure.CSV;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.Utilities;
using HouseholdAccountBook.Models.AppServices;
using HouseholdAccountBook.ViewModels.Abstract;
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
        public SelectorViewModel<KeyValuePair<int, string>, int> NationalHolidayTextEncodingSelectorVM => field ??= new(static p => p.Key);

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
        public SelectorViewModel<KeyValuePair<Log.LogLevel, string>, Log.LogLevel> LogLevelSelectorVM => field ??= new(static p => p.Key);

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
        #endregion

        #region コマンド
        /// <summary>
        /// 言語設定適用コマンド
        /// </summary>
        public ICommand RestartForLanguageCommand => field ??= new RelayCommand(this.RestartForLanguageCommand_Execute);
        /// <summary>
        /// 言語設定適用コマンド処理
        /// </summary>
        private void RestartForLanguageCommand_Execute()
        {
            if (MessageBox.Show(Properties.Resources.Message_RestartNotification, Properties.Resources.Title_Conformation,
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK) {
                UserSettingService.Instance.SelectedCaltureName = this.CultureNameSelectorVM.SelectedKey;

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
        public ICommand SaveOtherSettingsCommand => field ??= new RelayCommand(this.SaveOtherSettingsCommand_Execute);
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

            // バージョン情報
            this.SelectedIfNotifyLatestAtAppLaunched = UserSettingService.Instance.CheckLatestVersionAtAppLaunched;

            // 言語情報
            await this.CultureNameSelectorVM.LoadAsync(UserSettingService.Instance.SelectedCaltureName);

            // カレンダー情報
            this.InputedStartMonth = UserSettingService.Instance.FiscalStartMonth;
            CSVFileDao.HolidayCSVConfigulation holidayCsvConfig = UserSettingService.Instance.HolidayCSVConfig;
            this.InputedNationalHolidayCsvURI = holidayCsvConfig.Url;
            await this.NationalHolidayTextEncodingSelectorVM.LoadAsync(holidayCsvConfig.TextEncoding.CodePage);
            this.InputedNationalHolidayCsvDateIndex = holidayCsvConfig.DateIndex + 1;

            // ウィンドウ情報
            WindowLocationManager.Configuration locationConfig = UserSettingService.Instance.WindowLocationConfig;
            this.IsPositionSaved = locationConfig.IsPositionSaved;
            this.WindowSettingVMList = LoadWindowSettings();

            // ログ情報
            LogImpl.Configuration logConfig = UserSettingService.Instance.LogConfig;
            this.SelectedIfOutputOperationLog = logConfig.OutputLogToFile;
            this.InputedOperationLogNum = logConfig.LogFileAmount;
            await this.LogLevelSelectorVM.LoadAsync(logConfig.OutputLogLevel);
            WindowLog.Configulation windowLogConfig = UserSettingService.Instance.WindowLogConfig;
            this.SelectedIfOutputWindowLog = windowLogConfig.OutputLog;
            this.InputedWindowLogNum = windowLogConfig.LogFileAmount;
            ExceptionLog.Configuration exceptionLogConfig = UserSettingService.Instance.ExceptionLogConfig;
            this.InputedUnhandledExceptionLogNum = exceptionLogConfig.LogFileAmount;
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
        private void Save()
        {
            using FuncLog funcLog = new();

            // バージョン情報
            UserSettingService.Instance.CheckLatestVersionAtAppLaunched = this.SelectedIfNotifyLatestAtAppLaunched;

            // 言語情報
            UserSettingService.Instance.SelectedCaltureName = this.CultureNameSelectorVM.SelectedKey;

            // カレンダー情報
            UserSettingService.Instance.FiscalStartMonth = this.InputedStartMonth;
            UserSettingService.Instance.HolidayCSVConfig = new() {
                Url = this.InputedNationalHolidayCsvURI,
                TextEncoding = Encoding.GetEncoding(this.NationalHolidayTextEncodingSelectorVM.SelectedKey),
                DateIndex = this.InputedNationalHolidayCsvDateIndex - 1
            };

            // ウィンドウ情報
            UserSettingService.Instance.WindowLocationConfig = new() {
                IsPositionSaved = this.IsPositionSaved
            };

            // ログ情報
            UserSettingService.Instance.LogConfig = new() {
                OutputLogToFile = this.SelectedIfOutputOperationLog,
                LogFileAmount = this.InputedOperationLogNum,
                OutputLogLevel = this.LogLevelSelectorVM.SelectedKey
            };
            UserSettingService.Instance.WindowLogConfig = new() {
                OutputLog = this.SelectedIfOutputWindowLog,
                LogFileAmount = this.InputedWindowLogNum
            };
            UserSettingService.Instance.ExceptionLogConfig = new() {
                LogFileAmount = this.InputedUnhandledExceptionLogNum
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
