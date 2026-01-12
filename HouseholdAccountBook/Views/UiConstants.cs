using HouseholdAccountBook.Adapters.Logger;
using HouseholdAccountBook.Enums;
using HouseholdAccountBook.Views.Windows;
using System.Collections.Generic;

namespace HouseholdAccountBook.Views
{
    /// <summary>
    /// UI関連の定数定義
    /// </summary>
    public static class UiConstants
    {
        #region 種別表示文字列
        /// <summary>
        /// DB種別文字列
        /// </summary>
        public static Dictionary<DBKind, string> DBKindStr => new() {
            { DBKind.SQLite,        "SQLite" },
            { DBKind.PostgreSQL,    "PostgreSQL" },
//            { DBKind.Access,        "Access" },
        };
        /// <summary>
        /// 帳簿種別文字列
        /// </summary>
        public static Dictionary<BookKind, string> BookKindStr => new() {
            { BookKind.Uncategorized,       Properties.Resources.BookKind_Uncategorized },
            { BookKind.Wallet,              Properties.Resources.BookKind_Wallet },
            { BookKind.BankAccount,         Properties.Resources.BookKind_BankAccount },
            { BookKind.CreditCard,          Properties.Resources.BookKind_CreditCard },
            { BookKind.BrokerageAccount,    Properties.Resources.BookKind_BrokerageAccount }
        };
        /// <summary>
        /// 収支種別文字列
        /// </summary>
        public static Dictionary<BalanceKind, string> BalanceKindStr => new() {
            { BalanceKind.Income,   Properties.Resources.BalanceKind_Income },
            { BalanceKind.Expenses, Properties.Resources.BalanceKind_Expenses }
        };
        /// <summary>
        /// 休日設定種別文字列
        /// </summary>
        public static Dictionary<HolidaySettingKind, string> HolidaySettingKindStr => new() {
            { HolidaySettingKind.Nothing,       Properties.Resources.HolidaySettingKind_Nothing },
            { HolidaySettingKind.BeforeHoliday, Properties.Resources.HolidaySettingKind_BeforeHoliday },
            { HolidaySettingKind.AfterHoliday,  Properties.Resources.HolidaySettingKind_AfterHoliday }
        };
        /// <summary>
        /// 手数料種別文字列
        /// </summary>
        public static Dictionary<CommissionKind, string> CommissionKindStr => new() {
            { CommissionKind.MoveFrom,  Properties.Resources.CommissionKind_MoveFrom },
            { CommissionKind.MoveTo,    Properties.Resources.CommissionKind_MoveTo }
        };
        /// <summary>
        /// グラフ種別1文字列
        /// </summary>
        public static Dictionary<GraphKind1, string> GraphKind1Str => new() {
            { GraphKind1.IncomeAndExpensesGraph,    Properties.Resources.GraphKind1_IncomeAndExpensesGraph },
            { GraphKind1.BalanceGraph,              Properties.Resources.GraphKind1_BalanceGraph }
        };
        /// <summary>
        /// グラフ種別2文字列
        /// </summary>
        public static Dictionary<GraphKind2, string> GraphKind2Str => new() {
            { GraphKind2.CategoryGraph, Properties.Resources.GraphKind2_CategoryGraph },
            { GraphKind2.ItemGraph,     Properties.Resources.GraphKind2_ItemGraph }
        };
        /// <summary>
        /// ログレベル文字列
        /// </summary>
        public static Dictionary<Log.LogLevel, string> LogLevelStr => new() {
            { Log.LogLevel.Trace,       Properties.Resources.LogLevel_Trace  },
            { Log.LogLevel.Debug,       Properties.Resources.LogLevel_Debug },
            { Log.LogLevel.Info,        Properties.Resources.LogLevel_Information },
            { Log.LogLevel.Warn,        Properties.Resources.LogLevel_Warning },
            { Log.LogLevel.Error,       Properties.Resources.LogLevel_Error },
            { Log.LogLevel.Critical,    Properties.Resources.LogLevel_Critical }
        };
        #endregion

        /// <summary>
        /// 言語名文字列
        /// </summary>
        public static Dictionary<string, string> CultureNameStr => new() {
            { "ja-JP",  Properties.Resources.CultureName_ja_JP },
            { "en-001", Properties.Resources.CultureName_en_001 }
        };

        /// <summary>
        /// ウィンドウ名文字列
        /// </summary>
        public static Dictionary<string, string> WindowNameStr => new() {
            { nameof(DbSettingWindow),              "DbSetting" },
            { nameof(MainWindow),                   "Main" },
            { nameof(TermWindow),                   "Term" },
            { nameof(SettingsWindow),               "Settings" },
            { nameof(VersionWindow),                "Version" },
            { nameof(CsvComparisonWindow),          "CsvComp" },
            { nameof(MoveRegistrationWindow),       "MoveReg" },
            { nameof(ActionRegistrationWindow),     "ActReg" },
            { nameof(ActionListRegistrationWindow), "ActListReg" }
        };

    }
}
