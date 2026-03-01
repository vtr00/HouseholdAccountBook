using HouseholdAccountBook.Infrastructure.DB.DbDao.Compositions;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers.Abstract;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.Utilities.Extensions;
using HouseholdAccountBook.Models.ValueObjects;
using HouseholdAccountBook.ViewModels.Abstract;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HouseholdAccountBook.ViewModels.Windows
{
    /// <summary>
    /// 期間指定ウィンドウVM
    /// </summary>
    public class PeriodSelectionWindowViewModel : WindowViewModelBase
    {
        #region Bindingプロパティ
        /// <summary>
        /// 選択された月
        /// </summary>
        public DateTime SelectedMonth {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    this.SelectedStart = value.GetFirstDateOfMonth().ToDateOnly();
                }
            }
        }
        /// <summary>
        /// 選択された開始日
        /// </summary>
        #region SelectedStart
        public DateOnly SelectedStart {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    this.SelectedMonth = value.ToDateTime(TimeOnly.MinValue);
                    // TODO: SelectedStartよりも後になった場合の処理
                }
            }
        }
        #endregion
        /// <summary>
        /// 選択された終了日
        /// </summary>
        #region SelectedEnd
        public DateOnly SelectedEnd {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    // TODO: SelectedStartよりも前になった場合の処理
                }
            }
        }
        #endregion
        /// <summary>
        /// 選択された期間
        /// </summary>
        #region SelectedPeriod
        public PeriodObj<DateOnly> SelectedPeriod {
            get => new(this.SelectedStart, this.SelectedEnd);
            set {
                this.SelectedStart = value.Start;
                this.SelectedEnd = value.End;
            }
        }
        #endregion

        #region コマンド
        /// <summary>
        /// 今月コマンド
        /// </summary>
        public ICommand ThisMonthCommand => new RelayCommand(this.ThisMonthCommand_Executed);
        /// <summary>
        /// 全期間コマンド
        /// </summary>
        public ICommand AllPeriodCommand => new RelayCommand(this.AllPeriodCommand_Executed);
        #endregion
        #endregion

        #region コマンドイベントハンドラ
        /// <summary>
        /// 今月コマンド処理
        /// </summary>
        void ThisMonthCommand_Executed() => this.SelectedPeriod = new PeriodObj<DateOnly>(DateOnlyExtensions.Today.GetFirstDateOfMonth(), DateOnlyExtensions.Today.GetLastDateOfMonth());

        /// <summary>
        /// 全期間コマンド処理
        /// </summary>
        async void AllPeriodCommand_Executed()
        {
            var firstLastPair = await this.LoadFirstLastDate();
            this.SelectedPeriod = new PeriodObj<DateOnly>(firstLastPair.Item1, firstLastPair.Item2);
        }
        #endregion

        #region ウィンドウ設定プロパティ
        public override Point WindowPointSetting {
            get {
                Properties.Settings settings = Properties.Settings.Default;
                return new Point(settings.TermWindow_Left, settings.TermWindow_Top);
            }
            set {
                Properties.Settings settings = Properties.Settings.Default;
                settings.TermWindow_Left = value.X;
                settings.TermWindow_Top = value.Y;
                settings.Save();
            }
        }
        #endregion

        public override Task LoadAsync() => throw new NotImplementedException();

        public override void AddEventHandlers()
        {
            // NOP
        }

        /// <summary>
        /// 帳簿項目の初日/最終日を取得する
        /// </summary>
        /// <returns>初日/最終日のペア</returns>
        private async Task<Tuple<DateOnly, DateOnly>> LoadFirstLastDate()
        {
            using FuncLog funcLog = new();

            DateOnly firstDate;
            DateOnly lastDate;
            await using (DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync()) {
                PeriodInfoDao periodInfoDao = new(dbHandler);
                var dto = await periodInfoDao.Find();
                firstDate = DateOnly.FromDateTime(dto.FirstTime);
                lastDate = DateOnly.FromDateTime(dto.LastTime);
            }
            return new Tuple<DateOnly, DateOnly>(firstDate, lastDate);
        }
    }
}
