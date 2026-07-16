using HouseholdAccountBook.Infrastructure.DB.DbHandlers;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Models;
using HouseholdAccountBook.Models.Args;
using HouseholdAccountBook.Models.UiDto;
using HouseholdAccountBook.Models.ValueObjects;
using HouseholdAccountBook.ViewModels;
using HouseholdAccountBook.Views.Extensions;
using System;
using System.Collections.Generic;
using System.Windows;

namespace HouseholdAccountBook.Views.Windows
{
    /// <summary>
    /// ActionRegistrationWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ActionRegistrationWindow : Window
    {
        #region イベント
        /// <summary>
        /// 帳簿変更時のイベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<AccountIdObj>> SelectedAccountChanged {
            add => this.WVM.SelectedAccountChanged += value;
            remove => this.WVM.SelectedAccountChanged -= value;
        }
        /// <summary>
        /// 日時変更時のイベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<DateTime>> SelectedDateChanged {
            add => this.WVM.SelectedDateChanged += value;
            remove => this.WVM.SelectedDateChanged -= value;
        }

        /// <summary>
        /// 登録時のイベント
        /// </summary>
        public event EventHandler<EventArgs<IEnumerable<ActionIdObj>>> Registrated {
            add => this.WVM.Registrated += value;
            remove => this.WVM.Registrated -= value;
        }
        #endregion

        #region コンストラクタ
        /// <summary>
        /// 帳簿項目の追加のために <see cref="ActionRegistrationWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="owner">親ウィンドウ</param>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="initialAccountId">初期選択する帳簿のID</param>
        /// <param name="initialMonth">初期選択する年月</param>
        /// <param name="initialDate">初期選択する日付</param>
        public ActionRegistrationWindow(Window owner, DbHandlerFactory dbHandlerFactory, AccountIdObj initialAccountId, DateOnly? initialMonth, DateOnly? initialDate)
            : this(owner, dbHandlerFactory, initialAccountId, initialMonth, initialDate, null, null, RegistrationKind.Add) { }

        /// <summary>
        /// 帳簿項目の追加のために <see cref="ActionRegistrationWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="owner">親ウィンドウ</param>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="initialAccountId">初期選択する帳簿ID</param>
        /// <param name="initialRecord">初期表示するCSVレコード</param>
        public ActionRegistrationWindow(Window owner, DbHandlerFactory dbHandlerFactory, AccountIdObj initialAccountId, ActionCsvModel initialRecord)
            : this(owner, dbHandlerFactory, initialAccountId, null, null, initialRecord, null, RegistrationKind.Add) { }

        /// <summary>
        /// 帳簿項目の複製/編集のために <see cref="ActionRegistrationWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="owner">親ウィンドウ</param>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="targetActionId">複製/編集する帳簿項目のID</param>
        /// <param name="regKind">登録種別</param>
        public ActionRegistrationWindow(Window owner, DbHandlerFactory dbHandlerFactory, ActionIdObj targetActionId, RegistrationKind regKind = RegistrationKind.Edit)
            : this(owner, dbHandlerFactory, null, null, null, null, targetActionId, regKind) { }

        /// <summary>
        /// <see cref="ActionRegistrationWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="owner">親ウィンドウ</param>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="initialAccountId">初期選択する帳簿ID</param>
        /// <param name="initialMonth">初期選択する年月</param>
        /// <param name="initialDate">初期選択する日付</param>
        /// <param name="initialRecord">初期表示するCSVレコード</param>
        /// <param name="targetActionId">複製/編集する帳簿項目ID</param>
        /// <param name="regKind">登録種別</param>
        private ActionRegistrationWindow(Window owner, DbHandlerFactory dbHandlerFactory, AccountIdObj initialAccountId, DateOnly? initialMonth, DateOnly? initialDate,
                                         ActionCsvModel initialRecord, ActionIdObj targetActionId, RegistrationKind regKind)
        {
            using FuncLog funcLog = new(new { initialAccountId, initialMonth, initialDate, initialRecord, targetActionId, regKind });

            this.Owner = owner;
            this.Name = UiConstants.WindowNameStr[nameof(ActionRegistrationWindow)];
            WindowLocationManager.Instance.Add(this);

            this.InitializeComponent();
            this.AddCommonEventHandlersToVM();

            this.WVM.Initialize(dbHandlerFactory);
            this.WVM.RegKind = regKind;
            this.WVM.AddedByCsvComparison = initialRecord is not null;

            this.Loaded += async (sender, e) => {
                using FuncLog funcLog = new(methodName: nameof(this.Loaded));

                await this.WVM.LoadAsync(initialAccountId, initialMonth, initialDate, initialRecord, targetActionId);
                this.WVM.AddEventHandlers();
            };
        }
        #endregion
    }
}
