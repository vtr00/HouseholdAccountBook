using HouseholdAccountBook.Enums;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.Models.DbHandler;
using HouseholdAccountBook.Others;
using System;
using System.Collections.Generic;
using System.Windows;
using static HouseholdAccountBook.Extensions.FrameworkElementExtensions;

namespace HouseholdAccountBook.Views.Windows
{
    /// <summary>
    /// MoveRegistrationWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MoveRegistrationWindow : Window
    {
        #region イベント
        /// <summary>
        /// 登録時イベント
        /// </summary>
        public event EventHandler<EventArgs<List<int>>> Registrated
        {
            add => this.WVM.Registrated += value;
            remove => this.WVM.Registrated -= value;
        }
        #endregion

        #region コンストラクタ
        /// <summary>
        /// 帳簿項目(移動)の新規登録のために <see cref="MoveRegistrationWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="owner">親ウィンドウ</param>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="selectedBookId">選択された帳簿ID</param>
        /// <param name="selectedMonth">選択された年月</param>
        /// <param name="selectedDate">選択された日付</param>
        public MoveRegistrationWindow(Window owner, DbHandlerFactory dbHandlerFactory, int? selectedBookId, DateTime? selectedMonth, DateTime? selectedDate)
        {
            this.InitializeComponent();

            this.Owner = owner;
            this.LoadWindowSetting();

            this.AddCommonEventHandlers();
            this.Loaded += async (sender, e) => {
                using (WaitCursorManager wcm = this.GetWaitCursorManagerFactory().Create()) {
                    await this.WVM.LoadAsync(selectedBookId, null, selectedMonth, selectedDate);
                }
            };

            this.WVM.Initialize(this.GetWaitCursorManagerFactory(), dbHandlerFactory);
            this.WVM.RegKind = RegistrationKind.Add;
        }

        /// <summary>
        /// 帳簿項目(移動)の編集(複製)のために <see cref="MoveRegistrationWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="owner">親ウィンドウ</param>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="selectedGroupId">選択された帳簿項目のグループID</param>
        /// <param name="regKind">登録種別</param>
        public MoveRegistrationWindow(Window owner, DbHandlerFactory dbHandlerFactory, int selectedGroupId, RegistrationKind regKind = RegistrationKind.Edit)
        {
            this.InitializeComponent();

            this.Owner = owner;
            this.LoadWindowSetting();

            this.AddCommonEventHandlers();
            this.Loaded += async (sender, e) => {
                using (WaitCursorManager wcm = this.GetWaitCursorManagerFactory().Create()) {
                    await this.WVM.LoadAsync(null, selectedGroupId, null, null);
                }
            };

            this.WVM.Initialize(this.GetWaitCursorManagerFactory(), dbHandlerFactory);
            this.WVM.RegKind = regKind;
        }
        #endregion
    }
}
