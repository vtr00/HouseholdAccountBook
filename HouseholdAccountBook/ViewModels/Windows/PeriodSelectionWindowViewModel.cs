using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.Utilities.Extensions;
using HouseholdAccountBook.Models;
using HouseholdAccountBook.Models.AppServices;
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
        /// 選択期間種別
        /// </summary>
        public PeriodKind SelectedPeriodKind {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    switch (value) {
                        case PeriodKind.Monthly:
                            this.SelectedMonth = this.SelectedStart.ToDateTime(TimeOnly.MinValue);
                            break;
                        case PeriodKind.Selected:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// 選択された月
        /// </summary>
        public DateTime SelectedMonth {
            get => this.mSelectedMonth;
            set => this.SelectedPeriod = new(value.GetFirstDateOfMonth().ToDateOnly(), value.GetLastDateOfMonth().ToDateOnly());
        }
        private DateTime mSelectedMonth;

        /// <summary>
        /// 選択された開始日
        /// </summary>
        public DateOnly SelectedStart {
            get => this.mSelectedStart;
            set {
                DateOnly selectedEnd = this.SelectedEnd;
                if (selectedEnd < value) {
                    selectedEnd = value;
                }
                this.SelectedPeriod = new(value, selectedEnd);
            }
        }
        private DateOnly mSelectedStart;

        /// <summary>
        /// 選択された終了日
        /// </summary>
        public DateOnly SelectedEnd {
            get => this.mSelectedEnd;
            set {
                DateOnly selectedStart = this.SelectedStart;
                if (value < selectedStart) {
                    selectedStart = value;
                }
                this.SelectedPeriod = new(selectedStart, value);
            }
        }
        private DateOnly mSelectedEnd;

        /// <summary>
        /// 選択された期間
        /// </summary>
        public PeriodObj<DateOnly> SelectedPeriod {
            get => new(this.SelectedStart, this.SelectedEnd);
            set {
                this.mSelectedMonth = value.Start.ToDateTime(TimeOnly.MinValue);
                this.mSelectedStart = value.Start;
                this.mSelectedEnd = value.End;
                this.RaisePropertyChanged(nameof(this.SelectedMonth));
                this.RaisePropertyChanged(nameof(this.SelectedStart));
                this.RaisePropertyChanged(nameof(this.SelectedEnd));
            }
        }
        #endregion

        #region コマンド
        /// <summary>
        /// 今月コマンド
        /// </summary>
        public ICommand ThisMonthCommand => field ??= new RelayCommand(() => this.SelectedMonth = DateTime.Today, () => this.SelectedMonth != DateTime.Today.GetFirstDateOfMonth());

        /// <summary>
        /// 全期間コマンド
        /// </summary>
        public ICommand AllPeriodCommand => field ??= new AsyncRelayCommand(async () => this.SelectedPeriod = await this.LoadFirstLastDate(), null, this.mBusyService);
        #endregion

        #region ウィンドウ設定プロパティ
        public override Point WindowPointSetting {
            get => UserSettingService.Instance.TermWindowPoint;
            set => UserSettingService.Instance.TermWindowPoint = value;
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
        private async Task<PeriodObj<DateOnly>> LoadFirstLastDate()
        {
            using FuncLog funcLog = new();

            AppCommonService service = new(this.mDbHandlerFactory);
            return (await service.LoadPeriodOfAll()).Convert(DateOnly.FromDateTime);
        }
    }
}
