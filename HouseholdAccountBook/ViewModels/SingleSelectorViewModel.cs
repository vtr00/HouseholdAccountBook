using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.Utilities.Args;
using HouseholdAccountBook.Infrastructure.Utilities.Extensions;
using HouseholdAccountBook.ViewModels.Abstract;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// VMリストとその選択VMを保持するVM
    /// </summary>
    /// <typeparam name="VM">VM</typeparam>
    /// <typeparam name="KEY">VMを特定するキー(IDなど)</typeparam>
    public class SingleSelectorViewModel<VM, KEY> : BindableBase
        where VM : class
        where KEY : class
    {
        #region フィールド
        /// <summary>
        /// <see cref="VM"/> から <see cref="KEY"/> への変換処理
        /// </summary>
        private readonly Func<VM, KEY> mSelector;
        /// <summary>
        /// 選択VMがNullの場合でも変更を通知するか
        /// </summary>
        private readonly bool mItemChangedIfNull;
        /// <summary>
        /// 出力元ファイル名
        /// </summary>
        private readonly string mFileName;
        /// <summary>
        /// 出力元メンバー名
        /// </summary>
        private readonly string mMemberName;
        #endregion

        #region イベント
        /// <summary>
        /// 選択VM変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<KEY>> SelectedChanged;
        #endregion

        #region プロパティ
        /// <summary>
        /// <see cref="VM"> リスト読込処理(同期)
        /// </summary>
        public Func<IEnumerable<VM>> Loader { protected get; set; }
        /// <summary>
        /// <see cref="VM"> リスト読込処理(非同期)
        /// </summary>
        public Func<Task<IEnumerable<VM>>> LoaderAsync { protected get; set; }

        /// <summary>
        /// <see cref="VM"/> リスト
        /// </summary>
        public ObservableCollection<VM> ItemList { get; } = [];
        /// <summary>
        /// <see cref="VM"/> リストのアイテム数
        /// </summary>
        public int Count => this.ItemList.Count;

        /// <summary>
        /// 選択 <see cref="VM"/>
        /// </summary>
        public VM SelectedItem {
            get;
            set {
                VM old = field;
                if (this.SetProperty(ref field, value)) {
                    if (value != null || this.mItemChangedIfNull) {
                        this.OnSelectedChanged(old, value);
                    }
                }
            }
        }
        /// <summary>
        /// 選択 <see cref="KEY"/>
        /// </summary>
        public KEY SelectedKey => this.mSelector(this.SelectedItem);
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="selector"><see cref="KEY"/><see cref="VM"/> から <see cref="KEY"/> への変換処理</param>
        /// <param name="itemChangedIfNull">選択 <see cref="VM"/> がNullの場合でも変更を通知するか</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        public SingleSelectorViewModel(Func<VM, KEY> selector, bool itemChangedIfNull = false, [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = "")
        {
            using FuncLog funcLog = new(new { itemChangedIfNull, fileName, memberName });

            ArgumentNullException.ThrowIfNull(selector);

            this.mSelector = selector;
            this.mItemChangedIfNull = itemChangedIfNull;
            this.mFileName = fileName;
            this.mMemberName = memberName;
        }

        /// <summary>
        /// <see cref="VM"/> リストを読込み、指定の <see cref="VM"/> を選択する(非同期)
        /// </summary>
        /// <param name="selection"><see cref="VM"/> を選択するキー</param>
        /// <returns></returns>
        public async Task LoadAsync(KEY selection = null)
        {
            using FuncLog funcLog = new(new { selection }, fileName: this.mFileName, methodName: $"{this.mMemberName}.{nameof(LoadAsync)}");
            if (this.LoaderAsync is null) { throw new InvalidOperationException(); }

            KEY tmpSelected = selection ?? this.mSelector(this.SelectedItem);

            IEnumerable<VM> tmpList = await this.LoaderAsync.Invoke();
            this.ItemList.Clear();
            foreach(VM tmp in tmpList) {
                this.ItemList.Add(tmp);
            }
            // ジェネリックで安全に比較するために、 EqualityComparer<T>.Default を用いる
            this.SelectedItem = this.ItemList.FirstOrElementAtOrDefault(vm => EqualityComparer<KEY>.Default.Equals(this.mSelector(vm), tmpSelected), 0);
        }

        /// <summary>
        /// <see cref="VM"/> リストを読込み、指定の <see cref="VM"/> を選択する(同期)
        /// </summary>
        /// <param name="selection"><see cref="VM"/> を選択するキー</param>
        public void Load(KEY selection = null)
        {
            using FuncLog funcLog = new(new { selection }, fileName: this.mFileName, methodName: $"{this.mMemberName}.{nameof(Load)}");
            if (this.Loader is null) { throw new InvalidOperationException(); }

            KEY tmpSelected = selection ?? this.mSelector(this.SelectedItem);

            IEnumerable<VM> tmpList = this.Loader.Invoke();
            this.ItemList.Clear();
            foreach (VM tmp in tmpList) {
                this.ItemList.Add(tmp);
            }
            // ジェネリックで安全に比較するために、 EqualityComparer<T>.Default を用いる
            this.SelectedItem = this.ItemList.FirstOrElementAtOrDefault(vm => EqualityComparer<KEY>.Default.Equals(this.mSelector(vm), tmpSelected), 0);
        }

        /// <summary>
        /// <see cref="VM"/> リストの選択変更時処理
        /// </summary>
        /// <param name="oldVM">前回の選択 <see cref="VM"/></param>
        /// <param name="newVM">今回の選択 <see cref="VM"/></param>
        private void OnSelectedChanged(VM oldVM, VM newVM)
        {
            ChangedEventArgs<KEY> args = new() { OldValue = this.mSelector(oldVM), NewValue = this.mSelector(newVM) };
            using FuncLog funcLog = new(args, fileName:this.mFileName, methodName:$"{this.mMemberName}.{nameof(OnSelectedChanged)}");

            this.SelectedChanged?.Invoke(this, args);
        }
    }
}
