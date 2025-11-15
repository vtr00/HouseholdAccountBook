using HouseholdAccountBook.Adapters.DbHandler;
using HouseholdAccountBook.Adapters.Logger;
using HouseholdAccountBook.Enums;
using HouseholdAccountBook.Extensions;
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
        public event EventHandler<EventArgs<List<int>>> Registrated {
            add => this.WVM.Registrated += value;
            remove => this.WVM.Registrated -= value;
        }

        /// <summary>
        /// 帳簿変更時のイベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<int?>> BookChanged {
            add => this.WVM.BookChanged += value;
            remove => this.WVM.BookChanged -= value;
        }
        /// <summary>
        /// 日時変更時のイベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<DateTime>> DateChanged {
            add => this.WVM.DateChanged += value;
            remove => this.WVM.DateChanged -= value;
        }
        #endregion

        #region コンストラクタ
        /// <summary>
        /// 帳簿項目の追加のために <see cref="ActionRegistrationWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="owner">親ウィンドウ</param>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="initialBookId">初期選択する帳簿のID</param>
        /// <param name="initialMonth">初期選択する年月</param>
        /// <param name="initialDate">初期選択する日付</param>
        public ActionRegistrationWindow(Window owner, DbHandlerFactory dbHandlerFactory, int? initialBookId, DateTime? initialMonth, DateTime? initialDate)
            : this(owner, dbHandlerFactory, initialBookId, initialMonth, initialDate, null, null, RegistrationKind.Add) { }

        /// <summary>
        /// 帳簿項目の追加のために <see cref="ActionRegistrationWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="owner">親ウィンドウ</param>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="initialBookId">初期選択する帳簿ID</param>
        /// <param name="initialRecord">初期表示するCSVレコード</param>
        public ActionRegistrationWindow(Window owner, DbHandlerFactory dbHandlerFactory, int? initialBookId, CsvViewModel initialRecord)
            : this(owner, dbHandlerFactory, initialBookId, null, null, initialRecord, null, RegistrationKind.Add) { }

        /// <summary>
        /// 帳簿項目の複製/編集のために <see cref="ActionRegistrationWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="owner">親ウィンドウ</param>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="targetActionId">複製/編集する帳簿項目のID</param>
        /// <param name="regKind">登録種別</param>
        public ActionRegistrationWindow(Window owner, DbHandlerFactory dbHandlerFactory, int targetActionId, RegistrationKind regKind = RegistrationKind.Edit)
            : this(owner, dbHandlerFactory, null, null, null, null, targetActionId, regKind) { }

        /// <summary>
        /// <see cref="ActionRegistrationWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="owner">親ウィンドウ</param>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="initialBookId">初期選択する帳簿ID</param>
        /// <param name="initialMonth">初期選択する年月</param>
        /// <param name="initialDate">初期選択する日付</param>
        /// <param name="initialRecord">初期表示するCSVレコード</param>
        /// <param name="targetActionId">複製/編集する帳簿項目ID</param>
        /// <param name="regKind">登録種別</param>
        private ActionRegistrationWindow(Window owner, DbHandlerFactory dbHandlerFactory, int? initialBookId, DateTime? initialMonth, DateTime? initialDate,
                                         CsvViewModel initialRecord, int? targetActionId, RegistrationKind regKind)
        {
            using FuncLog funcLog = new(new { initialBookId, initialMonth, initialDate, initialRecord, targetActionId, regKind });

            this.Owner = owner;
            this.Name = "ActReg";
            WindowLocationManager.Instance.Add(this);

            this.InitializeComponent();
            this.AddCommonEventHandlersToVM();

            this.WVM.Initialize(this.GetWaitCursorManagerFactory(), dbHandlerFactory);
            this.WVM.RegKind = regKind;
            this.WVM.AddedByCsvComparison = initialRecord is not null;

            this.Loaded += async (sender, e) => {
                using FuncLog funcLog = new(methodName: nameof(this.Loaded));

                using (WaitCursorManager wcm = this.GetWaitCursorManagerFactory().Create(methodName: nameof(this.Loaded))) {
                    await this.WVM.LoadAsync(initialBookId, initialMonth, initialDate, initialRecord, targetActionId);
                }
                this.WVM.AddEventHandlers();
            };
        }
        #endregion
    }
}
