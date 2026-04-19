using HouseholdAccountBook.Models;
using HouseholdAccountBook.Models.UiDto;
using HouseholdAccountBook.Models.ValueObjects;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Component;
using System;
using System.Collections.Generic;

namespace HouseholdAccountBook.ViewModels.Settings
{
    /// <summary>
    /// 帳簿VM(設定用)
    /// </summary>
    public class BookSettingViewModel : BindableBase
    {
        #region プロパティ
        /// <summary>
        /// 帳簿モデル
        /// </summary>
        public BookModel Book { get; init; }

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
        }

        /// <summary>
        /// 帳簿種別セレクタVM
        /// </summary>
        public SelectorViewModel<KeyValuePair<BookKind, string>, BookKind> BookKindSelectorVM => field ??= new(static p => p.Key);

        /// <summary>
        /// 入力された備考
        /// </summary>
        public string InputedRemark {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// 入力された初期残高
        /// </summary>
        public decimal InputedInitialValue {
            get;
            set => this.SetProperty(ref field, value);
        }

        #region 期間情報
        /// <summary>
        /// 選択された開始日の有無
        /// </summary>
        public bool SelectedIfStartDateExists {
            get;
            set => this.SetProperty(ref field, value);
        }
        /// <summary>
        /// 指定された終了日の有無
        /// </summary>
        public bool SelectedIfEndDateExists {
            get;
            set => this.SetProperty(ref field, value);
        }
        /// <summary>
        /// 指定された期間
        /// </summary>
        public PeriodObj<DateOnly> InputedPeriod {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        #region 支払情報
        /// <summary>
        /// 支払の必要があるか
        /// </summary>
        public bool NeedToPay => this.BookKindSelectorVM.SelectedKey == BookKind.CreditCard;

        /// <summary>
        /// 入力された支払日
        /// </summary>
        public int? InputedPayDay {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// 支払元帳簿セレクタVM
        /// </summary>
        public SelectorViewModel<BookModel, BookIdObj> DebitBookSelectorVM => field ??= new(static vm => vm?.Id);
        #endregion

        #region CSV情報
        /// <summary>
        /// CSVデータがあるか
        /// </summary>
        public bool CsvDataExists => this.BookKindSelectorVM.SelectedKey != BookKind.Wallet;

        /// <summary>
        /// 入力されたCSVフォルダパス
        /// </summary>
        public string InputedCsvFolderPath {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// 文字エンコーディング セレクタVM
        /// </summary>
        public SelectorViewModel<KeyValuePair<int, string>, int> TextEncodingSelectorVM => field ??= new(static p => p.Key);

        /// <summary>
        /// 入力された日付 位置(1開始)
        /// </summary>
        public int? InputedActDateIndex {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// 入力された支出 位置(1開始)
        /// </summary>
        public int? InputedExpensesIndex {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// 入力された項目名 位置(1開始)
        /// </summary>
        public int? InputedItemNameIndex {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 関連性セレクタVM
        /// </summary>
        public SelectorViewModel<RelationViewModel, ItemIdObj> RelationSelectorVM => field ??= new(static vm => vm?.Id);
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="book">帳簿Model</param>
        public BookSettingViewModel(BookModel book)
        {
            this.Book = book;
            this.InputedName = book.Name;
            this.InputedRemark = book.Remark;
            this.InputedInitialValue = book.InitialValue;
            this.InputedPayDay = book.PayDay;
            this.SelectedIfStartDateExists = book.StartDateExists;
            this.SelectedIfEndDateExists = book.EndDateExists;
            this.InputedPeriod = book.Period;
            this.InputedCsvFolderPath = book.CsvFolderPath;
            this.InputedItemNameIndex = book.ItemNameIndex;
            this.InputedActDateIndex = book.ActDateIndex;
            this.InputedExpensesIndex = book.ExpensesIndex;

            this.BookKindSelectorVM.SelectionChanged += (sender, e) => {
                this.RaisePropertyChanged(nameof(this.NeedToPay));
                this.RaisePropertyChanged(nameof(this.CsvDataExists));
            };
        }
    }
}
