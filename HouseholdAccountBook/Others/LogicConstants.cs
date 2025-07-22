namespace HouseholdAccountBook.Others
{
    /// <summary>
    /// ビジネスロジック関連の定数定義
    /// </summary>
    public static class LogicConstants
    {
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
    }
}
