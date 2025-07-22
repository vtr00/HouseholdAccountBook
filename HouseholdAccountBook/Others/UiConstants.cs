using System.Collections.Generic;
using static HouseholdAccountBook.Others.LogicConstants;
using static HouseholdAccountBook.Others.DbConstants;

namespace HouseholdAccountBook.Others
{
    /// <summary>
    /// UI関連の定数定義
    /// </summary>
    public static class UiConstants
    {
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

        #region 種別表示文字列
        /// <summary>
        /// DB種別文字列
        /// </summary>
        public static Dictionary<DBKind, string> DBKindStr => new Dictionary<DBKind, string>() {
            { DBKind.PostgreSQL,    "PostgreSQL" },
            { DBKind.Access,        "Access" },
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
