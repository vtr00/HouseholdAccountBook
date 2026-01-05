using HouseholdAccountBook.Adapters.DbHandler;
using HouseholdAccountBook.Adapters.Logger;
using HouseholdAccountBook.Enums;
using HouseholdAccountBook.Extensions;
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
        public event EventHandler<EventArgs<List<int>>> Registrated {
            add => this.WVM.Registrated += value;
            remove => this.WVM.Registrated -= value;
        }
        #endregion

        #region コンストラクタ
        /// <summary>
        /// 帳簿項目(移動)の追加のために <see cref="MoveRegistrationWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="owner">親ウィンドウ</param>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="initialBookId">初期選択する帳簿ID</param>
        /// <param name="initialMonth">初期選択する年月</param>
        /// <param name="initialDate">初期選択する日付</param>
        public MoveRegistrationWindow(Window owner, DbHandlerFactory dbHandlerFactory, int? initialBookId, DateTime? initialMonth, DateTime? initialDate)
            : this(owner, dbHandlerFactory, initialBookId, initialMonth, initialDate, null, RegistrationKind.Add) { }

        /// <summary>
        /// 帳簿項目(移動)の複製/編集のために <see cref="MoveRegistrationWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="owner">親ウィンドウ</param>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="targetGroupId">複製/編集時、複製/編集対象の帳簿項目のグループID</param>
        /// <param name="regKind">登録種別</param>
        public MoveRegistrationWindow(Window owner, DbHandlerFactory dbHandlerFactory, int targetGroupId, RegistrationKind regKind = RegistrationKind.Edit)
            : this(owner, dbHandlerFactory, null, null, null, targetGroupId, regKind) { }

        /// <summary>
        /// 帳簿項目(移動)の複製/編集のために <see cref="MoveRegistrationWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="owner">親ウィンドウ</param>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="initialBookId">追加時、初期選択する帳簿ID</param>
        /// <param name="initialMonth">追加時、初期選択する年月</param>
        /// <param name="initialDate">追加時、初期選択する日付</param>
        /// <param name="targetGroupId">複製/編集時、複製/編集対象の帳簿項目のグループID</param>
        /// <param name="regKind">登録種別</param>
        private MoveRegistrationWindow(Window owner, DbHandlerFactory dbHandlerFactory, int? initialBookId, DateTime? initialMonth, DateTime? initialDate, int? targetGroupId,
            RegistrationKind regKind)
        {
            using FuncLog funcLog = new(new { initialBookId, initialMonth, initialDate, targetGroupId, regKind });

            this.Owner = owner;
            this.Name = UiConstants.WindowNameStr[nameof(MoveRegistrationWindow)];
            WindowLocationManager.Instance.Add(this);

            this.InitializeComponent();
            this.AddCommonEventHandlersToVM();

            this.WVM.Initialize(this.GetWaitCursorManagerFactory(), dbHandlerFactory);
            this.WVM.RegKind = regKind;

            this.Loaded += async (sender, e) => {
                using FuncLog funcLog = new(methodName: nameof(this.Loaded));

                using (WaitCursorManager wcm = this.GetWaitCursorManagerFactory().Create(methodName: nameof(this.Loaded))) {
                    await this.WVM.LoadAsync(initialBookId, initialMonth, initialDate, targetGroupId);
                }
                this.WVM.AddEventHandlers();
            };
        }
        #endregion
    }
}
