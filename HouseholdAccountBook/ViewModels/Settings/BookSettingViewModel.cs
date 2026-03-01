using HouseholdAccountBook.Models;
using HouseholdAccountBook.Models.DomainModels;
using HouseholdAccountBook.Models.Utilities.Extensions;
using HouseholdAccountBook.Models.ValueObjects;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Component;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HouseholdAccountBook.ViewModels.Settings
{
    /// <summary>
    /// 帳簿VM(設定用)
    /// </summary>
    public class BookSettingViewModel : BindableBase
    {
        #region プロパティ
        /// <summary>
        /// 帳簿ID
        /// </summary>
        public BookIdObj Id { get; init; }

        /// <summary>
        /// ソート順
        /// </summary>
        public int SortOrder { get; init; }

        /// <summary>
        /// 入力された帳簿名
        /// </summary>
        #region InputedName
        public string InputedName {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 帳簿種別辞書
        /// </summary>
        public Dictionary<BookKind, string> BookKindDic { get; } = UiConstants.BookKindStr;
        /// <summary>
        /// 選択された帳簿種別
        /// </summary>
        #region SelectedBookKind
        public BookKind SelectedBookKind {
            get;
            set {
                _ = this.SetProperty(ref field, value);
                this.RaisePropertyChanged(nameof(this.NeedToPay));
                this.RaisePropertyChanged(nameof(this.CsvDataExists));
            }
        } = BookKind.Uncategorized;
        #endregion

        /// <summary>
        /// 入力された備考
        /// </summary>
        #region InputedRemark
        public string InputedRemark {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 入力された初期残高
        /// </summary>
        #region InputedInitialValue
        public decimal InputedInitialValue {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        #region 期間情報
        /// <summary>
        /// 選択された開始日の有無
        /// </summary>
        #region SelectedIfStartDateExists
        public bool SelectedIfStartDateExists {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        /// <summary>
        /// 指定された終了日の有無
        /// </summary>
        #region SelectedIfEndDateExists
        public bool SelectedIfEndDateExists {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        /// <summary>
        /// 指定された期間
        /// </summary>
        #region InputedPeriod
        public PeriodObj<DateOnly> InputedPeriod {
            get;
            set => this.SetProperty(ref field, value);
        } = new(DateOnlyExtensions.Today, DateOnlyExtensions.Today);
        #endregion
        #endregion

        #region 支払情報
        /// <summary>
        /// 支払の必要があるか
        /// </summary>
        #region NeedToPay
        public bool NeedToPay => this.SelectedBookKind == BookKind.CreditCard;
        #endregion

        /// <summary>
        /// 支払元帳簿VMリスト
        /// </summary>
        #region DebitBookVMList
        public ObservableCollection<BookModel> DebitBookVMList {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        /// <summary>
        /// 選択された支払元帳簿VM
        /// </summary>
        #region SelectedDebitBookVM
        public BookModel SelectedDebitBookVM {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 入力された支払日
        /// </summary>
        #region InputedPayDay
        public int? InputedPayDay {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        #endregion

        #region CSV情報
        /// <summary>
        /// CSVデータがあるか
        /// </summary>
        public bool CsvDataExists => this.SelectedBookKind != BookKind.Wallet;

        /// <summary>
        /// 入力されたCSVフォルダパス
        /// </summary>
        #region InputedCsvFolderPath
        public string InputedCsvFolderPath {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 文字エンコーディング
        /// </summary>
        #region TextEncodingList
        public ObservableCollection<KeyValuePair<int, string>> TextEncodingList {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        /// <summary>
        /// 選択された文字エンコーディング
        /// </summary>
        #region SelectedTextEncoding
        public int SelectedTextEncoding {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 入力された日付 位置(1開始)
        /// </summary>
        #region InputedActDateIndex
        public int? InputedActDateIndex {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 入力された支出 位置(1開始)
        /// </summary>
        #region InputedExpensesIndex
        public int? InputedExpensesIndex {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 入力された項目名 位置(1開始)
        /// </summary>
        #region InputedItemNameIndex
        public int? InputedItemNameIndex {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        #endregion

        /// <summary>
        /// 関係性VMリスト
        /// </summary>
        #region RelationVMList
        public ObservableCollection<RelationViewModel> RelationVMList {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        /// <summary>
        /// 選択された関係性VM
        /// </summary>
        #region SelectedRelationVM
        public RelationViewModel SelectedRelationVM {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        #endregion
    }
}
