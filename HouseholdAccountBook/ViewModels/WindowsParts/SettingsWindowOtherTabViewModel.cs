using HouseholdAccountBook.Adapters.Logger;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static HouseholdAccountBook.Extensions.EncodingExtensions;
using static HouseholdAccountBook.Views.UiConstants;

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
        /// 言語種別辞書
        /// </summary>
        public Dictionary<string, string> CultureNameDic { get; } = CultureNameStr;
        /// <summary>
        /// 選択された言語種別
        /// </summary>
        #region SelectedCultureName
        public string SelectedCultureName {
            get;
            set => this.SetProperty(ref field, value);
        } = "ja-JP";
        #endregion
        #endregion

        #region カレンダー
        /// <summary>
        /// 開始月
        /// </summary>
        #region StartMonth
        public int StartMonth {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 国民の祝日CSV URI
        /// </summary>
        #region NationalHolidayCsvURI
        public string NationalHolidayCsvURI {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 国民の祝日CSV 文字エンコーディング
        /// </summary>
        #region NatioalHolidayTextEncodingList
        public ObservableCollection<KeyValuePair<int, string>> NationalHolidayTextEncodingList {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        /// <summary>
        /// 国民の祝日CSV 選択された文字エンコーディング
        /// </summary>
        #region SelectedNationalHolidayTextEncoding
        public int SelectedNationalHolidayTextEncoding {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 国民の祝日CSV 日付インデックス(1開始)
        /// </summary>
        #region NationalHolidayCsvDateIndex
        public int NationalHolidayCsvDateIndex {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        #endregion

        #region ウィンドウ
        /// <summary>
        /// ウィンドウ位置を保存するか
        /// </summary>
        #region IsPositionSaved
        public bool IsPositionSaved {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// ウィンドウ設定
        /// </summary>
        #region WindowSettingVMList
        public ObservableCollection<WindowSettingViewModel> WindowSettingVMList {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        #endregion

        #region ログ
        /// <summary>
        /// 操作ログ出力
        /// </summary>
        #region OutputOperationLog
        public bool OutputOperationLog {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        /// <summary>
        /// 操作ログ数
        /// </summary>
        #region OperationLogNum
        public int OperationLogNum {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        /// <summary>
        /// ログレベル辞書
        /// </summary>
        #region LogLevelDic
        public Dictionary<Log.LogLevel, string> LogLevelDic {
            get {
                Dictionary<Log.LogLevel, string> dic = LogLevelStr;
#if !DEBUG
                _ = dic.Remove(Log.LogLevel.Trace);
#endif
                return dic;
            }
        }
        #endregion
        /// <summary>
        /// 選択されたログレベル
        /// </summary>
        #region SelectedLogLevel
        public Log.LogLevel SelectedLogLevel {
            get;
            set => this.SetProperty(ref field, value);
        } = Log.LogLevel.Debug;
        #endregion

        /// <summary>
        /// ウィンドウログ出力
        /// </summary>
        #region OutputWindowLog
        public bool OutputWindowLog {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        /// <summary>
        /// ウィンドウログ数
        /// </summary>
        #region WindowLogNum
        public int WindowLogNum {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 捕捉されない例外ログ数
        /// </summary>
        #region UnhandledExceptionLogNum
        public int UnhandledExceptionLogNum {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        #endregion

        #region コマンド
        /// <summary>
        /// 言語設定適用コマンド
        /// </summary>
        public ICommand RestartForLanguageCommand => new RelayCommand(this.RestartForLanguageCommand_Executed);
        /// <summary>
        /// ウィンドウ設定再読込コマンド
        /// </summary>
        public ICommand ReloadWindowSettingCommand => new RelayCommand(this.ReloadWindowSettingCommand_Executed);
        /// <summary>
        /// ウィンドウ設定初期化コマンド
        /// </summary>
        public ICommand InitializeWindowSettingCommand => new RelayCommand(this.InitializeWindowSettingCommand_Executed);
        /// <summary>
        /// その他設定保存コマンド
        /// </summary>
        public ICommand SaveOtherSettingsCommand => new RelayCommand(this.SaveOtherSettingsCommand_Executed);
        #endregion
        #endregion

        #region コマンドイベントハンドラ
        /// <summary>
        /// 言語設定適用コマンド処理
        /// </summary>
        private void RestartForLanguageCommand_Executed()
        {
            if (MessageBox.Show(Properties.Resources.Message_RestartNotification, Properties.Resources.Title_Conformation,
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK) {
                Properties.Settings.Default.App_CultureName = this.SelectedCultureName;
                Properties.Settings.Default.Save();

                ((App)Application.Current).Restart();
            }
        }

        /// <summary>
        /// ウィンドウ設定再読込コマンド処理
        /// </summary>
        private void ReloadWindowSettingCommand_Executed() => this.WindowSettingVMList = LoadWindowSettings();

        /// <summary>
        /// ウィンドウ設定初期化コマンド処理
        /// </summary>
        private void InitializeWindowSettingCommand_Executed()
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
                // settings.VersionWindow_Width = -1;
                // settings.VersionWindow_Height = -1;

                settings.App_InitSizeFlag = true;
                settings.Save();

                ((App)Application.Current).Restart();
            }
        }

        /// <summary>
        /// その他設定保存コマンド処理
        /// </summary>
        private void SaveOtherSettingsCommand_Executed()
        {
            this.Save();
            this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        public override Task LoadAsync() => throw new NotImplementedException();

        /// <summary>
        /// その他設定を読み込む
        /// </summary>
        public void Load()
        {
            using FuncLog funcLog = new();

            Properties.Settings settings = Properties.Settings.Default;

            // 言語情報
            this.SelectedCultureName = settings.App_CultureName;

            // カレンダー情報
            this.StartMonth = settings.App_StartMonth;
            this.NationalHolidayCsvURI = settings.App_NationalHolidayCsv_Uri;
            this.NationalHolidayTextEncodingList = GetTextEncodingList();
            this.SelectedNationalHolidayTextEncoding = settings.App_NationalHolidayCsv_TextEncoding;
            this.NationalHolidayCsvDateIndex = settings.App_NationalHolidayCsv_DateIndex + 1;

            // ウィンドウ情報
            this.IsPositionSaved = settings.App_IsPositionSaved;
            this.WindowSettingVMList = LoadWindowSettings();

            // ログ情報
            this.OutputOperationLog = settings.App_OutputFlag_OperationLog;
            this.OperationLogNum = settings.App_OperationLogNum;
            this.SelectedLogLevel = (Log.LogLevel)settings.App_OperationLogLevel;
            this.OutputWindowLog = settings.App_OutputFlag_WindowLog;
            this.WindowLogNum = settings.App_WindowLogNum;
            this.UnhandledExceptionLogNum = settings.App_UnhandledExceptionLogNum;
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
                    Title = Properties.Resources.Title_TermSelectionWindow,
                    Left = settings.TermWindow_Left, Top = settings.TermWindow_Top,
                    Width = -1, Height = -1
                },
                new WindowSettingViewModel(){
                    Title = Properties.Resources.Title_VersionWindow,
                    Left = settings.VersionWindow_Left, Top = settings.VersionWindow_Top,
                    Width = -1, Height = -1
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

            // 言語情報
            settings.App_CultureName = this.SelectedCultureName;

            // カレンダー情報
            settings.App_StartMonth = this.StartMonth;
            settings.App_NationalHolidayCsv_Uri = this.NationalHolidayCsvURI;
            settings.App_NationalHolidayCsv_TextEncoding = this.SelectedNationalHolidayTextEncoding;
            settings.App_NationalHolidayCsv_DateIndex = this.NationalHolidayCsvDateIndex - 1;

            // ウィンドウ情報
            settings.App_IsPositionSaved = this.IsPositionSaved;

            // ログ情報
            settings.App_OutputFlag_OperationLog = this.OutputOperationLog;
            settings.App_OperationLogNum = this.OperationLogNum;
            settings.App_OperationLogLevel = (int)this.SelectedLogLevel;
            settings.App_OutputFlag_WindowLog = this.OutputWindowLog;
            settings.App_WindowLogNum = this.WindowLogNum;
            settings.App_UnhandledExceptionLogNum = this.UnhandledExceptionLogNum;

            Log.OutputLogLevel = this.SelectedLogLevel;
            settings.Save();

            // 新しい設定に合わせて古いログファイルを削除する
            Log.DeleteOldLogFiles();
            ExceptionLog.DeleteOldExceptionLogs();
            WindowLog.DeleteAllOldWindowLogs();
        }
    }
}
