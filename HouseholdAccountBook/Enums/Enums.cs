namespace HouseholdAccountBook.Enums
{
    #region DB関連
    /// <summary>
    /// SQL DB種別
    /// </summary>
    public enum DBKind
    {
        /// <summary>
        /// 未定義
        /// </summary>
        Undefined = -1,
        /// <summary>
        /// PostgreSQL
        /// </summary>
        PostgreSQL = 0,
        /// <summary>
        /// Access
        /// </summary>
        Access,
        /// <summary>
        /// SQLite
        /// </summary>
        SQLite
    }

    /// <summary>
    /// SQL DBライブラリ種別
    /// </summary>
    public enum DBLibraryKind
    {
        /// <summary>
        /// 未定義
        /// </summary>
        Undefined = -1,
        /// <summary>
        /// PostgreSQL
        /// </summary>
        PostgreSQL = 0,
        /// <summary>
        /// Ole DB
        /// </summary>
        OleDb,
        /// <summary>
        /// SQLite
        /// </summary>
        SQLite
    }

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
    #endregion

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
        /// DB設定タブ
        /// </summary>
        DbSettingsTab = 2,
        /// <summary>
        /// その他タブ
        /// </summary>
        OtherSettingsTab = 3
    }
    #endregion

    /// <summary>
    /// ファイルパス種別
    /// </summary>
    public enum FilePathKind
    {
        DumpExeFile,
        RestoreExeFile,
        DbFile
    }
}
