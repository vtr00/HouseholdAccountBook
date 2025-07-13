using System;
using System.Collections.Generic;

namespace HouseholdAccountBook.ConstValue
{
    /// <summary>
    /// 定数定義
    /// </summary>
    public static class ConstValue
    {
        #region DB
        /// <summary>
        /// 更新者
        /// </summary>
        public static string Updater { get; } = string.Empty;
        /// <summary>
        /// 挿入者
        /// </summary>
        public static string Inserter { get; } = string.Empty;
        /// <summary>
        /// PostgreSQL パスワード入力方法
        /// </summary>
        public enum PostgresPasswordInput
        {
            /// <summary>
            /// InputWindowによる入力
            /// </summary>
            InputWindow = 0,
            /// <summary>
            /// pgpass.confによる入力
            /// </summary>
            PgPassConf = 1
        }
        /// <summary>
        /// PostgreSQL ダンプ/リストアフォーマット
        /// </summary>
        public enum PostgresFormat
        {
            Plain = 0,
            Custom = 1,
            Dictionary = 2,
            Tar = 3
        }
        #endregion

        #region ログ
        /// <summary>
        /// ログファイルのフォルダパス
        /// </summary>
        public static string LogFolderPath = @".\Logs";
        /// <summary>
        /// ログファイルパス
        /// </summary>
        public static string LogFilePath
        {
            get {
                DateTime dt = App.StartupTime;
                return string.Format($@"{LogFolderPath}\{dt:yyyyMMdd_HHmmss}.txt");
            }
        }
        #endregion

        #region バックアップファイル
        /// <summary>
        /// バックアップファイル名
        /// </summary>
        public static string BackupFileName
        {
            get {
                DateTime dt = DateTime.Now;
                return string.Format($"{dt:yyyyMMdd_HHmmss}.backup");
            }
        }
        #endregion

        #region 捕捉されない例外情報
        /// <summary>
        /// 捕捉されない例外情報のファイルのフォルダパス
        /// </summary>
        public static string UnhandledExceptionInfoFolderPath = @".\UnhandledExceptions";
        /// <summary>
        /// 捕捉されない例外情報のファイルパス
        /// </summary>
        public static string UnhandledExceptionInfoFilePath
        {
            get {
                DateTime dt = DateTime.Now;
                return string.Format($@"{UnhandledExceptionInfoFolderPath}\{dt:yyyyMMdd_HHmmss}.txt");
            }
        }
        #endregion

        #region ウィンドウ情報
        /// <summary>
        /// ウィンドウ情報のファイルのフォルダパス
        /// </summary>
        public static string WindowLocationFolderPath = @".\WindowLocations";
        /// <summary>
        /// ウィンドウ情報のファイルパス
        /// </summary>
        public static string WindowLocationFilePath(String windowName)
        {
            DateTime dt = App.StartupTime;
            return string.Format($@"{WindowLocationFolderPath}\{windowName}_{dt:yyyyMMdd_HHmmss}.txt");
        }
        #endregion

        #region タブ
        /// <summary>
        /// タブ
        /// </summary>
        /// <remarks>数字はタブのインデックスと対応付けること</remarks>
        public enum Tabs
        {
            /// <summary>
            /// 帳簿タブ
            /// </summary>
            BooksTab = 0,
            /// <summary>
            /// 日別グラフタブ
            /// </summary>
            DailyGraphTab = 1,
            /// <summary>
            /// 月別一覧タブ
            /// </summary>
            MonthlyListTab = 2,
            /// <summary>
            /// 月別グラフタブ
            /// </summary>
            MonthlyGraphTab = 3,
            /// <summary>
            /// 年別一覧タブ
            /// </summary>
            YearlyListTab = 4,
            /// <summary>
            /// 年別グラフタブ
            /// </summary>
            YearlyGraphTab = 5
        }

        /// <summary>
        /// 設定タブ
        /// </summary>
        /// <remarks>数字は設定タブのインデックスと対応付けること</remarks>
        public enum SettingsTabs
        {
            /// <summary>
            /// 項目設定タブ
            /// </summary>
            ItemSettingsTab = 0,
            /// <summary>
            /// 帳簿設定タブ
            /// </summary>
            BookSettingsTab = 1,
            /// <summary>
            /// その他タブ
            /// </summary>
            OtherSettingsTab = 2
        }
        #endregion

        #region 種別
        /// <summary>
        /// SQLデータベース種別
        /// </summary>
        public enum  DBKind
        {
            /// <summary>
            /// PostgreSQL
            /// </summary>
            PostgreSQL,
            /// <summary>
            /// OLE DB
            /// </summary>
            OleDb,
            /// <summary>
            /// SQL Server
            /// </summary>
            SQLite
        }
        /// <summary>
        /// 期間種別
        /// </summary>
        public enum TermKind
        {
            /// <summary>
            /// 月
            /// </summary>
            Monthly,
            /// <summary>
            /// 選択
            /// </summary>
            Selected
        }
        /// <summary>
        /// 帳簿種別
        /// </summary>
        public enum BookKind
        {
            /// <summary>
            /// 未分類
            /// </summary>
            Uncategorized = 0,
            /// <summary>
            /// 財布
            /// </summary>
            Wallet = 1,
            /// <summary>
            /// 銀行口座
            /// </summary>
            BankAccount = 2,
            /// <summary>
            /// クレジットカード
            /// </summary>
            CreditCard = 3,
            /// <summary>
            /// 株式口座
            /// </summary>
            BrokerageAccount = 4
        }
        /// <summary>
        /// 収支種別
        /// </summary>
        public enum BalanceKind
        {
            /// <summary>
            /// 収入
            /// </summary>
            Income = 0,
            /// <summary>
            /// 支出
            /// </summary>
            Expenses = 1,
            /// <summary>
            /// その他(残高、差引損益)
            /// </summary>
            Others = -1
        }
        /// <summary>
        /// 休日設定種別
        /// </summary>
        public enum HolidaySettingKind
        {
            /// <summary>
            /// なし
            /// </summary>
            Nothing,
            /// <summary>
            /// 休日前
            /// </summary>
            BeforeHoliday,
            /// <summary>
            /// 休日後
            /// </summary>
            AfterHoliday,
        }
        /// <summary>
        /// グループ種別
        /// </summary>
        public enum GroupKind
        {
            /// <summary>
            /// 移動
            /// </summary>
            Move = 0,
            /// <summary>
            /// 繰返し
            /// </summary>
            Repeat = 1,
            /// <summary>
            /// リスト登録
            /// </summary>
            ListReg = 2
        }
        /// <summary>
        /// 手数料種別
        /// </summary>
        public enum CommissionKind
        {
            /// <summary>
            /// 支払元負担
            /// </summary>
            MoveFrom = 0,
            /// <summary>
            /// 支払先負担
            /// </summary>
            MoveTo = 1
        }
        /// <summary>
        /// 検索種別
        /// </summary>
        public enum FindKind
        {
            /// <summary>
            /// 表示なし
            /// </summary>
            None = 0,
            /// <summary>
            /// 検索
            /// </summary>
            Find = 1,
            /// <summary>
            /// 置換
            /// </summary>
            Replace = 2
        }
        /// <summary>
        /// グラフ種別1
        /// </summary>
        public enum GraphKind1
        {
            /// <summary>
            /// 収支グラフ
            /// </summary>
            IncomeAndExpensesGraph = 0,
            /// <summary>
            /// 残高グラフ
            /// </summary>
            BalanceGraph = 1
        }
        /// <summary>
        /// グラフ種別2
        /// </summary>
        public enum GraphKind2
        {
            /// <summary>
            /// 分類グラフ
            /// </summary>
            CategoryGraph = 0,
            /// <summary>
            /// 項目グラフ
            /// </summary>
            ItemGraph = 1
        }
        /// <summary>
        /// 登録種別
        /// </summary>
        public enum RegistrationKind
        {
            /// <summary>
            /// 追加
            /// </summary>
            Add,
            /// <summary>
            /// 編集
            /// </summary>
            Edit,
            /// <summary>
            /// 複製
            /// </summary>
            Copy,
        }
        #endregion

        #region 種別文字列
        /// <summary>
        /// DB種別文字列
        /// </summary>
        public static Dictionary<DBKind, string> DBKindStr => new Dictionary<DBKind, string>() {
            { DBKind.PostgreSQL,    "PostgreSQL" },
            { DBKind.OleDb,         "Ole DB" },
            { DBKind.SQLite,        "SQLite" }
        };
        /// <summary>
        /// 帳簿種別文字列
        /// </summary>
        public static Dictionary<BookKind, string> BookKindStr => new Dictionary<BookKind, string>() {
            { BookKind.Uncategorized,       Properties.Resources.BookKind_Uncategorized },
            { BookKind.Wallet,              Properties.Resources.BookKind_Wallet },
            { BookKind.BankAccount,         Properties.Resources.BookKind_BankAccount },
            { BookKind.CreditCard,          Properties.Resources.BookKind_CreditCard },
            { BookKind.BrokerageAccount,    Properties.Resources.BookKind_BrokerageAccount }
        };
        /// <summary>
        /// 収支種別文字列
        /// </summary>
        public static Dictionary<BalanceKind, string> BalanceKindStr => new Dictionary<BalanceKind, string>() {
            { BalanceKind.Income,   Properties.Resources.BalanceKind_Income },
            { BalanceKind.Expenses, Properties.Resources.BalanceKind_Expenses }
        };
        /// <summary>
        /// 休日設定種別文字列
        /// </summary>
        public static Dictionary<HolidaySettingKind, string> HolidaySettingKindStr => new Dictionary<HolidaySettingKind, string>() {
            { HolidaySettingKind.Nothing,       Properties.Resources.HolidaySettingKind_Nothing },
            { HolidaySettingKind.BeforeHoliday, Properties.Resources.HolidaySettingKind_BeforeHoliday },
            { HolidaySettingKind.AfterHoliday,  Properties.Resources.HolidaySettingKind_AfterHoliday }
        };
        /// <summary>
        /// 手数料種別文字列
        /// </summary>
        public static Dictionary<CommissionKind, string> CommissionKindStr => new Dictionary<CommissionKind, string>() {
            { CommissionKind.MoveFrom,  Properties.Resources.CommissionKind_MoveFrom },
            { CommissionKind.MoveTo,    Properties.Resources.CommissionKind_MoveTo }
        };
        /// <summary>
        /// グラフ種別1文字列
        /// </summary>
        public static Dictionary<GraphKind1, string> GraphKind1Str => new Dictionary<GraphKind1, string>() {
            { GraphKind1.IncomeAndExpensesGraph,    Properties.Resources.GraphKind1_IncomeAndExpensesGraph },
            { GraphKind1.BalanceGraph,              Properties.Resources.GraphKind1_BalanceGraph }
        };
        /// <summary>
        /// グラフ種別2文字列
        /// </summary>
        public static Dictionary<GraphKind2, string> GraphKind2Str => new Dictionary<GraphKind2, string>() {
            { GraphKind2.CategoryGraph, Properties.Resources.GraphKind2_CategoryGraph },
            { GraphKind2.ItemGraph,     Properties.Resources.GraphKind2_ItemGraph }
        };
        /// <summary>
        /// 言語名文字列
        /// </summary>
        public static Dictionary<string, string> CultureNameStr => new Dictionary<string, string>() {
            { "ja-JP",  Properties.Resources.CultureName_ja_JP },
            { "en-001", Properties.Resources.CultureName_en_001 }
        };
        #endregion
    }
}
