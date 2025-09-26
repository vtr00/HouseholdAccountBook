using HouseholdAccountBook.Enums;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.Models.DbHandler;
using HouseholdAccountBook.Others;
using HouseholdAccountBook.ViewModels.Component;
using System;
using System.Collections.Generic;
using System.Windows;
using static HouseholdAccountBook.Extensions.FrameworkElementExtensions;

namespace HouseholdAccountBook.Views.Windows
{
    /// <summary>
    /// ActionRegistrationWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ActionRegistrationWindow : Window
    {
        #region イベント
        /// <summary>
        /// 登録時のイベント
        /// </summary>
        public event EventHandler<EventArgs<List<int>>> Registrated
        {
            add => this.WVM.Registrated += value;
            remove => this.WVM.Registrated -= value;
        }

        /// <summary>
        /// 帳簿変更時のイベント
        /// </summary>
        public event EventHandler<EventArgs<int?>> BookChanged
        {
            add => this.WVM.BookChanged += value;
            remove => this.WVM.BookChanged -= value;
        }
        /// <summary>
        /// 日時変更時のイベント
        /// </summary>
        public event EventHandler<EventArgs<DateTime>> DateChanged
        {
            add => this.WVM.DateChanged += value;
            remove => this.WVM.DateChanged -= value;
        }
        #endregion

        #region コンストラクタ
        /// <summary>
        /// 帳簿項目の新規登録のために <see cref="ActionRegistrationWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="owner">親ウィンドウ</param>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="selectedBookId">選択された帳簿ID</param>
        /// <param name="selectedMonth">選択された年月</param>
        /// <param name="selectedDate">選択された日付</param>
        public ActionRegistrationWindow(Window owner, DbHandlerFactory dbHandlerFactory, int? selectedBookId, DateTime? selectedMonth, DateTime? selectedDate)
        {
            this.InitializeComponent();

            this.Owner = owner;
            this.LoadWindowSetting();

            this.AddCommonEventHandlers();
            this.Loaded += async (sender, e) => {
                using (WaitCursorManager wcm = this.GetWaitCursorManagerFactory().Create()) {
                    await this.WVM.LoadAsync(selectedBookId, selectedMonth, selectedDate, null, null);
                }
            };

            this.WVM.Initialize(this.GetWaitCursorManagerFactory(), dbHandlerFactory);
            this.WVM.RegKind = RegistrationKind.Add;
        }

        /// <summary>
        /// 帳簿項目の新規登録のために <see cref="ActionRegistrationWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="owner">親ウィンドウ</param>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="selectedBookId">選択された帳簿ID</param>
        /// <param name="selectedRecord">選択されたCSVレコード</param>
        public ActionRegistrationWindow(Window owner, DbHandlerFactory dbHandlerFactory, int? selectedBookId, CsvViewModel selectedRecord)
        {
            this.InitializeComponent();

            this.Owner = owner;
            this.LoadWindowSetting();

            this.AddCommonEventHandlers();
            this.Loaded += async (sender, e) => {
                using (WaitCursorManager wcm = this.GetWaitCursorManagerFactory().Create()) {
                    await this.WVM.LoadAsync(selectedBookId, null, null, selectedRecord, null);
                }
            };

            this.WVM.Initialize(this.GetWaitCursorManagerFactory(), dbHandlerFactory);
            this.WVM.RegKind = RegistrationKind.Add;
            this.WVM.AddedByCsvComparison = true;
        }

        /// <summary>
        /// 帳簿項目の編集(複製)のために <see cref="ActionRegistrationWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="owner">親ウィンドウ</param>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="selectedActionId">帳簿項目ID</param>
        /// <param name="regKind">登録種別</param>
        public ActionRegistrationWindow(Window owner, DbHandlerFactory dbHandlerFactory, int selectedActionId, RegistrationKind regKind = RegistrationKind.Edit)
        {
            this.InitializeComponent();

            this.Owner = owner;
            this.LoadWindowSetting();

            this.AddCommonEventHandlers();
            this.Loaded += async (sender, e) => {
                using (WaitCursorManager wcm = this.GetWaitCursorManagerFactory().Create()) {
                    await this.WVM.LoadAsync(null, null, null, null, selectedActionId);
                }
            };

            this.WVM.Initialize(this.GetWaitCursorManagerFactory(), dbHandlerFactory);
            this.WVM.RegKind = regKind;
        }
        #endregion
    }
}
