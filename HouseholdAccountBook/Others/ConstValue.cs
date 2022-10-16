using System;
using System.Collections.Generic;

namespace HouseholdAccountBook.ConstValue
{
    /// <summary>
    /// 定数定義
    /// </summary>
    public static class ConstValue
    {
        /// <summary>
        /// 更新者
        /// </summary>
        public static string Updater { get; } = "";
        /// <summary>
        /// 挿入者
        /// </summary>
        public static string Inserter { get; } = "";

        /// <summary>
        /// 補足されない例外の情報ファイル名
        /// </summary>
        public static string UnhandledExceptionInfoFileName {
            get {
                DateTime dt = DateTime.Now;
                return string.Format("UnhandledException_{0}_{1}.txt", dt.ToString("yyMMdd"), dt.ToString("hhmmss"));
            }
        }

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
            /// 株式
            /// </summary>
            Stock = 4
        }
        /// <summary>
        /// 帳簿種別文字列
        /// </summary>
        public static Dictionary<BookKind, string> BookKindStr { get; } = new Dictionary<BookKind, string>() {
            { BookKind.Uncategorized, "未分類" }, { BookKind.Wallet, "財布" }, { BookKind.BankAccount, "銀行口座" },
            { BookKind.CreditCard, "クレジットカード" }, {BookKind.Stock, "株式" }
        };

        /// <summary>
        /// 区間種別
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
            Selected,
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
            Outgo = 1,
            /// <summary>
            /// その他(残高、差引損益)
            /// </summary>
            Others = -1
        }
        /// <summary>
        /// 収支種別文字列
        /// </summary>
        public static Dictionary<BalanceKind, string> BalanceKindStr { get; } = new Dictionary<BalanceKind, string>() {
            { BalanceKind.Income, "収入" }, { BalanceKind.Outgo, "支出" }
        };

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
        /// 休日設定種別文字列
        /// </summary>
        public static Dictionary<HolidaySettingKind, string> HolidaySettingKindStr { get; } = new Dictionary<HolidaySettingKind, string>() {
            { HolidaySettingKind.Nothing, "なし" }, { HolidaySettingKind.BeforeHoliday, "休日前" }, { HolidaySettingKind.AfterHoliday, "休日後" }
        };

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
            FromBook = 0,
            /// <summary>
            /// 支払先負担
            /// </summary>
            ToBook = 1
        }
        /// <summary>
        /// 手数料種別文字列
        /// </summary>
        public static Dictionary<CommissionKind, string> CommissionKindStr { get; } = new Dictionary<CommissionKind, string>() {
            { CommissionKind.FromBook, "移動元" }, { CommissionKind.ToBook, "移動先" }
        };

        /// <summary>
        /// グラフ種別
        /// </summary>
        public enum GraphKind
        {
            /// <summary>
            /// 収支
            /// </summary>
            IncomeAndOutgo = 0,
            /// <summary>
            /// 残高
            /// </summary>
            Balance = 1
        }
        /// <summary>
        /// グラフ種別文字列
        /// </summary>
        public static Dictionary<GraphKind, string> GraphKindStr { get; } = new Dictionary<GraphKind, string>() {
            { GraphKind.IncomeAndOutgo, "収支グラフ" }, { GraphKind.Balance, "残高グラフ" }
        };

        /// <summary>
        /// 登録モード
        /// </summary>
        public enum RegistrationMode
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

        /// <summary>
        /// メッセージタイトルテキスト
        /// </summary>
        public static class MessageTitle
        {
            /// <summary>
            /// 情報
            /// </summary>
            public static string Information { get; } = "情報";
            /// <summary>
            /// 警告
            /// </summary>
            public static string Exclamation { get; } = "警告";
            /// <summary>
            /// エラー
            /// </summary>
            public static string Error { get; } = "エラー";
        }

        /// <summary>
        /// メッセージテキスト
        /// </summary>
        public static class MessageText
        {
            /// <summary>
            /// インポートが完了しました。
            /// </summary>
            public static string FinishToSave { get; } = "保存が完了しました。";
            /// <summary>
            /// インポートが完了しました。
            /// </summary>
            public static string FinishToImport { get; } = "インポートが完了しました。";
            /// <summary>
            /// インポートに失敗しました。
            /// </summary>
            public static string FoultToImport { get; } = "インポートに失敗しました。";
            /// <summary>
            /// エクスポートが完了しました。
            /// </summary>
            public static string FinishToExport { get; } = "エクスポートが完了しました。";
            /// <summary>
            /// エクスポートに失敗しました。
            /// </summary>
            public static string FoultToExport { get; } = "エクスポートに失敗しました。";
            /// <summary>
            /// バックアップが完了しました。
            /// </summary>
            public static string FinishToBackUp { get; } = "バックアップが完了しました。";
            /// <summary>
            /// 選択した項目を削除しますか？
            /// </summary>
            public static string DeleteNotification { get; } = "選択した項目を削除します。よろしいですか？";
            /// <summary>
            /// 既存のデータを削除します。よろしいですか？
            /// </summary>
            public static string DeleteOldDataNotification { get; } = "既存のデータを削除します。よろしいですか？";
            /// <summary>
            /// 再起動します。よろしいですか？
            /// </summary>
            public static string RestartNotification { get; } = "再起動します。よろしいですか？";
            /// <summary>
            /// ハンドルされていない例外が発生しました。クリックで例外の情報を確認できます。
            /// </summary>
            public static string UnhandledExceptionOccured { get; } = "ハンドルされていない例外が発生しました。クリックで例外の情報を確認できます。";
        }
    }
}
