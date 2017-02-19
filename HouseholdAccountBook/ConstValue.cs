﻿using System.Collections.Generic;

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
        public static string Updater = "";
        /// <summary>
        /// 挿入者
        /// </summary>
        public static string Inserter = "";

        /// <summary>
        /// タブ
        /// </summary>
        /// <remarks>数字はタブのインデックスと対応付けること</remarks>
        public enum Tab
        {
            /// <summary>
            /// 帳簿タブ
            /// </summary>
            BookTab = 0,
            /// <summary>
            /// 年間一覧タブ
            /// </summary>
            ListTab = 1,
            /// <summary>
            /// グラフタブ
            /// </summary>
            GraphTab = 2,
            /// <summary>
            /// 設定タブ
            /// </summary>
            SettingTab = 3
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
        public static Dictionary<BalanceKind, string> BalanceStr = new Dictionary<BalanceKind, string>() {
            { BalanceKind.Income, "収入" }, { BalanceKind.Outgo, "支出" }
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
            Repeat = 1
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
        public static Dictionary<CommissionKind, string> CommissionStr = new Dictionary<CommissionKind, string>() {
            { CommissionKind.FromBook, "支払元" }, { CommissionKind.ToBook, "支払先" }
        };

        /// <summary>
        /// メッセージタイトルテキスト
        /// </summary>
        public static class MessageTitle
        {
            /// <summary>
            /// 情報
            /// </summary>
            public static string Information = "情報";
            /// <summary>
            /// 警告
            /// </summary>
            public static string Exclamation = "警告";
            /// <summary>
            /// エラー
            /// </summary>
            public static string Error = "エラー";
        }

        /// <summary>
        /// メッセージテキスト
        /// </summary>
        public static class Message
        {
            /// <summary>
            /// 接続に失敗しました。
            /// </summary>
            public static string ConnectionError = "接続に失敗しました。";
            /// <summary>
            /// 移動元と移動先の帳簿が同じです。
            /// </summary>
            public static string IllegalSameBook = "移動元と移動先の帳簿が同じです。";
            /// <summary>
            /// 金額が不正です。
            /// </summary>
            public static string IllegalValue = "金額が不正です。";
            /// <summary>
            /// インポートが完了しました。
            /// </summary>
            public static string FinishToImport = "インポートが完了しました。";
            /// <summary>
            /// インポートに失敗しました。
            /// </summary>
            public static string FoultToImport = "インポートに失敗しました。";
            /// <summary>
            /// エクスポートが完了しました。
            /// </summary>
            public static string FinishToExport = "エクスポートが完了しました。";
            /// <summary>
            /// エクスポートに失敗しました。
            /// </summary>
            public static string FoultToExport = "エクスポートに失敗しました。";
        }
    }
}
