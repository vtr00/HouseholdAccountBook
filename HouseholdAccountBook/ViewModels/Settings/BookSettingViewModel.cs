using HouseholdAccountBook.Models;
using HouseholdAccountBook.Models.UiDto;
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
    /// <param name="book">帳簿Model</param>
    public class BookSettingViewModel(BookModel book) : BindableBase
    {
        #region プロパティ

        public BookModel Book { get; init; } = book;

        /// <summary>
        /// 帳簿ID
        /// </summary>
        public BookIdObj Id => this.Book.Id;

        /// <summary>
        /// ソート順
        /// </summary>
        public int SortOrder => this.Book.SortOrder;

        /// <summary>
        /// 入力された帳簿名
        /// </summary>
        public string InputedName {
            get;
            set => this.SetProperty(ref field, value);
        } = book.Name;

        /// <summary>
        /// 帳簿種別辞書
        /// </summary>
        public Dictionary<BookKind, string> BookKindDic { get; } = UiConstants.BookKindStr;
        /// <summary>
        /// 選択された帳簿種別
        /// </summary>
        public BookKind SelectedBookKind {
            get;
            set {
                _ = this.SetProperty(ref field, value);
                this.RaisePropertyChanged(nameof(this.NeedToPay));
                this.RaisePropertyChanged(nameof(this.CsvDataExists));
            }
        } = book.BookKind;

        /// <summary>
        /// 入力された備考
        /// </summary>
        public string InputedRemark {
            get;
            set => this.SetProperty(ref field, value);
        } = book.Remark;

        /// <summary>
        /// 入力された初期残高
        /// </summary>
        public decimal InputedInitialValue {
            get;
            set => this.SetProperty(ref field, value);
        } = book.InitialValue;

        #region 期間情報
        /// <summary>
        /// 選択された開始日の有無
        /// </summary>
        public bool SelectedIfStartDateExists {
            get;
            set => this.SetProperty(ref field, value);
        } = book.StartDateExists;
        /// <summary>
        /// 指定された終了日の有無
        /// </summary>
        public bool SelectedIfEndDateExists {
            get;
            set => this.SetProperty(ref field, value);
        } = book.EndDateExists;
        /// <summary>
        /// 指定された期間
        /// </summary>
        public PeriodObj<DateOnly> InputedPeriod {
            get;
            set => this.SetProperty(ref field, value);
        } = book.Period;
        #endregion

        #region 支払情報
        /// <summary>
        /// 支払の必要があるか
        /// </summary>
        public bool NeedToPay => this.SelectedBookKind == BookKind.CreditCard;

        /// <summary>
        /// 入力された支払日
        /// </summary>
        public int? InputedPayDay {
            get;
            set => this.SetProperty(ref field, value);
        } = book.PayDay;

        /// <summary>
        /// 支払元帳簿VMリスト
        /// </summary>
        public ObservableCollection<BookModel> DebitBookVMList {
            get;
            set => this.SetProperty(ref field, value);
        }
        /// <summary>
        /// 選択された支払元帳簿VM
        /// </summary>
        public BookModel SelectedDebitBookVM {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        #region CSV情報
        /// <summary>
        /// CSVデータがあるか
        /// </summary>
        public bool CsvDataExists => this.SelectedBookKind != BookKind.Wallet;

        /// <summary>
        /// 入力されたCSVフォルダパス
        /// </summary>
        public string InputedCsvFolderPath {
            get;
            set => this.SetProperty(ref field, value);
        } = book.CsvFolderPath;

        /// <summary>
        /// 文字エンコーディング
        /// </summary>
        public ObservableCollection<KeyValuePair<int, string>> TextEncodingList {
            get;
            set => this.SetProperty(ref field, value);
        }
        /// <summary>
        /// 選択された文字エンコーディング
        /// </summary>
        public int SelectedTextEncoding {
            get;
            set => this.SetProperty(ref field, value);
        } = book.TextEncoding;

        /// <summary>
        /// 入力された日付 位置(1開始)
        /// </summary>
        public int? InputedActDateIndex {
            get;
            set => this.SetProperty(ref field, value);
        } = book.ActDateIndex;

        /// <summary>
        /// 入力された支出 位置(1開始)
        /// </summary>
        public int? InputedExpensesIndex {
            get;
            set => this.SetProperty(ref field, value);
        } = book.ExpensesIndex;

        /// <summary>
        /// 入力された項目名 位置(1開始)
        /// </summary>
        public int? InputedItemNameIndex {
            get;
            set => this.SetProperty(ref field, value);
        } = book.ItemNameIndex;
        #endregion

        /// <summary>
        /// 関係性VMリスト
        /// </summary>
        public ObservableCollection<RelationModel> RelationVMList {
            get;
            set => this.SetProperty(ref field, value);
        }
        /// <summary>
        /// 選択された関係性VM
        /// </summary>
        public RelationViewModel SelectedRelationVM {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
    }
}
