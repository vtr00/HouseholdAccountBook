using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.Utilities.Args;
using HouseholdAccountBook.Infrastructure.Utilities.Extensions;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// <see cref="SelectorViewModel"/> で <see cref="KEY"/> を選択するモード
    /// </summary>
    public enum SelectorMode
    {
        // 一致する項目がなければデフォルト(KEYがクラスなら選択なし状態)を選択する
        FirstOrDefault,
        // 一致する項目がなければ先頭の項目を選択する
        FirstOrElementAtOrDefault
    }

    /// <summary>
    /// VMリストとその選択VMを保持するVM
    /// </summary>
    /// <typeparam name="VM">VM</typeparam>
    /// <typeparam name="KEY">VMを特定するキー(IDなど)</typeparam>
    public class SelectorViewModel<VM, KEY> : BindableBase, ILoadableAsync
    {

        #region フィールド
        /// <summary>
        /// <see cref="VM"/> から <see cref="KEY"/> への変換処理
        /// </summary>
        private readonly Func<VM?, KEY?> mSelector;
        /// <summary>
        /// 選択 <see cref="VM"/> がNullの場合でも変更を通知するか
        /// </summary>
        private readonly bool mItemChangedIfNull;
        /// <summary>
        /// 出力元ファイル名
        /// </summary>
        private readonly string? mFileName;
        /// <summary>
        /// 出力元メンバー名
        /// </summary>
        private readonly string mMemberName;

        /// <summary>
        /// <see cref="VM"> リスト読込可能か
        /// </summary>
        private Func<bool>? mCanLoad;
        /// <summary>
        /// [同期] <see cref="VM"> リスト読込処理
        /// </summary>
        private Func<FuncLog, IEnumerable<VM>>? mLoad;
        /// <summary>
        /// [非同期] <see cref="VM"> リスト読込処理
        /// </summary>
        private Func<FuncLog, CancellationToken, Task<IEnumerable<VM>>>? mLoadAsync;
        /// <summary>
        /// 読込処理中か
        /// </summary>
        private bool mOnLoad = false;

        /// <summary>
        /// 多重読込処理防止トークン源
        /// </summary>
        private CancellationTokenSource? mSelectionCts;
        #endregion

        #region イベント
        /// <summary>
        /// 選択 <see cref="VM"> 変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<KEY>>? SelectionChanged;
        #endregion

        #region プロパティ
        /// <summary>
        /// <see cref="VM"/> リスト
        /// </summary>
        public ObservableCollection<VM> ItemList { get; } = [];
        /// <summary>
        /// <see cref="VM"/> リストのアイテム数
        /// </summary>
        public int Count => this.ItemList.Count;

        /// <summary>
        /// 非同期読込可能な子リスト
        /// </summary>
        public List<ILoadableAsync> Children { get; } = [];

        /// <summary>
        /// 選択 <see cref="VM"/>
        /// </summary>
        public VM? SelectedItem {
            get;
            set {
                KEY? old = this.SelectedKey;
                if (this.SetProperty(ref field, value)) {
                    // 読込処理中以外か
                    if (!this.mOnLoad) {
                        // NULLの場合に通知するか
                        if (this.SelectedKey != null || this.mItemChangedIfNull) {
                            // 待機せずデタッチして選択時の処理をする
                            _ = this.OnSelectionChangedAsync(old, this.SelectedKey);
                        }
                    }

                    this.RaisePropertyChanged(nameof(this.SelectedKey));
                    this.RaisePropertyChanged(nameof(this.SelectedIndex));
                }
            }
        }
        /// <summary>
        /// 選択 <see cref="KEY"/>
        /// </summary>
        public KEY? SelectedKey {
            get => this.mSelector.Invoke(this.SelectedItem) ?? default;
            set => this.SelectedItem = this.ItemList.FirstOrElementAtOrDefault(vm => EqualityComparer<KEY>.Default.Equals(this.mSelector.Invoke(vm), value), 0);
        }
        /// <summary>
        /// 選択インデックス
        /// </summary>
        public int SelectedIndex {
            get => this.SelectedItem == null ? -1 : this.ItemList.IndexOf(this.SelectedItem);
            set => this.SelectedItem = (value < 0 || this.ItemList.Count <= value) ? default : this.ItemList[value];
        }

        /// <summary>
        /// WaitCursorマネージャファクトリ
        /// </summary>
        public WaitCursorManagerFactory? WaitCursorManagerFactory { get; set; }
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="selector"><see cref="KEY"/><see cref="VM"/> から <see cref="KEY"/> への変換処理</param>
        /// <param name="itemChangedIfNull">選択 <see cref="VM"/> がNullの場合でも変更を通知するか</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        public SelectorViewModel(Func<VM?, KEY?> selector, bool itemChangedIfNull = false, [CallerFilePath] string? fileName = null, [CallerMemberName] string memberName = "")
        {
            using FuncLog funcLog = new(new { itemChangedIfNull, fileName, memberName });

            ArgumentNullException.ThrowIfNull(selector);

            this.mSelector = selector;
            this.mItemChangedIfNull = itemChangedIfNull;
            this.mFileName = fileName;
            this.mMemberName = memberName;
        }

        /// <summary>
        /// <see cref="VM"> リスト読込処理を設定する
        /// </summary>
        /// <param name="load">[同期] <see cref="VM"/> リスト読込処理</param>
        /// <param name="canLoad">Load可能判定処理</param>
        public void SetLoader(Func<IEnumerable<VM>> load, Func<bool>? canLoad = null) => this.SetLoader(funcLog => load(), canLoad);
        /// <summary>
        /// <see cref="VM"> リスト読込処理を設定する
        /// </summary>
        /// <param name="load">[同期] <see cref="VM"/> リスト読込処理</param>
        /// <param name="canLoad">Load可能判定処理</param>
        public void SetLoader(Func<FuncLog, IEnumerable<VM>> load, Func<bool>? canLoad = null)
        {
            using FuncLog funcLog = new(fileName: this.mFileName, methodName: $"{this.mMemberName}.{nameof(SetLoader)}");

            this.mCanLoad = canLoad;
            this.mLoad = load;
            this.mLoadAsync = null;
        }

        /// <summary>
        /// <see cref="VM"> リスト読込処理を設定する
        /// </summary>
        /// <param name="loadAsync">[非同期] <see cref="VM"> リスト読込処理</param>
        /// <param name="canLoad">Load可能判定処理</param>
        public void SetLoader(Func<Task<IEnumerable<VM>>> loadAsync, Func<bool>? canLoad = null) => this.SetLoader((funcLog, token) => loadAsync(), canLoad);
        /// <summary>
        /// <see cref="VM"> リスト読込処理を設定する
        /// </summary>
        /// <param name="loadAsync">[非同期] <see cref="VM"> リスト読込処理</param>
        /// <param name="canLoad">Load可能判定処理</param>
        public void SetLoader(Func<CancellationToken, Task<IEnumerable<VM>>> loadAsync, Func<bool>? canLoad = null) => this.SetLoader((funcLog, token) => loadAsync(token), canLoad);
        /// <summary>
        /// <see cref="VM"> リスト読込処理を設定する
        /// </summary>
        /// <param name="loadAsync">[非同期] <see cref="VM"> リスト読込処理</param>
        /// <param name="canLoad">Load可能判定処理</param>
        public void SetLoader(Func<FuncLog, CancellationToken, Task<IEnumerable<VM>>> loadAsync, Func<bool>? canLoad = null)
        {
            using FuncLog funcLog = new(fileName: this.mFileName, methodName: $"{this.mMemberName}.{nameof(SetLoader)}");

            this.mCanLoad = canLoad;
            this.mLoad = null;
            this.mLoadAsync = loadAsync;
        }

        /// <summary>
        /// [非同期] <see cref="VM"/> リストを読込む
        /// </summary>
        /// <param name="token">キャンセル用トークン</param>
        /// <returns></returns>
        public async Task LoadAsync(CancellationToken token = default) => await this.LoadAsync(default, token);
        /// <summary>
        /// [非同期] <see cref="VM"/> リストを読込み、指定の <see cref="VM"/> を選択する
        /// </summary>
        /// <param name="selection"><see cref="VM"/> を選択するキー. nullの場合は現在選択中の <see cref="VM"/> を選択する</param>
        /// <param name="token">キャンセル用トークン</param>
        /// <returns></returns>
        public async Task LoadAsync(KEY? selection, CancellationToken token) => await this.LoadAsync(selection, SelectorMode.FirstOrElementAtOrDefault, token);
        /// <summary>
        /// [非同期] <see cref="VM"/> リストを読込み、指定の <see cref="VM"/> を選択する
        /// </summary>
        /// <param name="selection"><see cref="VM"/> を選択するキー. nullの場合は現在選択中の <see cref="VM"/> を選択する</param>
        /// <param name="mode"><see cref="VM"> を選択するモード</param>
        /// <param name="token">キャンセル用トークン</param>
        /// <returns></returns>
        public async Task LoadAsync(KEY? selection, SelectorMode mode = SelectorMode.FirstOrElementAtOrDefault, CancellationToken token = default)
        {
            using FuncLog funcLog = new(new { selection }, fileName: this.mFileName, methodName: $"{this.mMemberName}.{nameof(LoadAsync)}");
            if (this.mLoad is null && this.mLoadAsync is null) { throw new InvalidOperationException(); }
            if (this.mLoad is not null && this.mLoadAsync is not null) { throw new InvalidOperationException(); }

            if (!(this.mCanLoad?.Invoke() ?? true)) {
                return;
            }

            using WaitCursorManager? wcm = this.WaitCursorManagerFactory?.Create(fileName: this.mFileName, methodName: $"{this.mMemberName}.{nameof(LoadAsync)}");

            this.mOnLoad = true;
            try {
                // リスト読込前の値を取得する
                KEY? oldKey = this.SelectedKey;

                // 表示するVMを取得する
                token.ThrowIfCancellationRequested();
                IEnumerable<VM> tmpList = [];
                if (this.mLoad is not null) {
                    tmpList = this.mLoad.Invoke(funcLog);
                }
                if (this.mLoadAsync is not null) {
                    tmpList = await this.mLoadAsync.Invoke(funcLog, token);
                }
                token.ThrowIfCancellationRequested();

                // 一旦未選択状態にする
                this.SelectedItem = default;

                // 取得したVMを反映する
                this.ItemList.Clear();
                foreach (VM tmp in tmpList) {
                    this.ItemList.Add(tmp);
                }

                KEY? tmpSelection = selection ?? oldKey;
                // ジェネリックで安全に比較するために、 EqualityComparer<T>.Default を用いる
                this.SelectedItem = mode switch {
                    SelectorMode.FirstOrElementAtOrDefault => this.ItemList.FirstOrElementAtOrDefault(vm => EqualityComparer<KEY>.Default.Equals(this.mSelector(vm), tmpSelection), 0),
                    SelectorMode.FirstOrDefault => this.ItemList.FirstOrDefault(vm => EqualityComparer<KEY>.Default.Equals(this.mSelector(vm), tmpSelection)),
                    _ => throw new NotImplementedException()
                };
                if (!EqualityComparer<KEY>.Default.Equals(this.SelectedKey, oldKey)) {
                    // NULLの場合に通知するか
                    if (this.SelectedItem != null || this.mItemChangedIfNull) {
                        // 子の読込処理を待機するためにここで呼ぶ
                        await this.OnSelectionChangedAsync(oldKey, this.SelectedKey, token);
                    }
                    else {
                        Log.Debug($"{nameof(this.SelectedItem)} is null");
                    }
                }
            }
            finally {
                this.mOnLoad = false;
            }
        }

        /// <summary>
        /// <see cref="VM"/> リストの選択変更時処理
        /// </summary>
        /// <param name="oldVM">前回の選択 <see cref="VM"/></param>
        /// <param name="newVM">今回の選択 <see cref="VM"/></param>
        protected async Task OnSelectionChangedAsync(KEY? oldKey, KEY? newKey, CancellationToken token = default)
        {
            ChangedEventArgs<KEY> args = new() { OldValue = oldKey, NewValue = newKey };
            using FuncLog funcLog = new(args, fileName: this.mFileName, methodName: $"{this.mMemberName}.{nameof(OnSelectionChangedAsync)}");

            using WaitCursorManager? wcm = this.WaitCursorManagerFactory?.Create(fileName: this.mFileName, methodName: $"{this.mMemberName}.{nameof(OnSelectionChangedAsync)}");

            if (token == default) {
                this.mSelectionCts?.Cancel();
                this.mSelectionCts?.Dispose();
                this.mSelectionCts = new();
                token = this.mSelectionCts.Token;
            }

            this.SelectionChanged?.Invoke(this, args);

            try {
                Task[] tasks = [.. this.Children.Select(async c => {
                    token.ThrowIfCancellationRequested();
                    await c.LoadAsync(token);
                    token.ThrowIfCancellationRequested();
                })];
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException) { }
        }
    }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="itemChangedIfNull">選択 <see cref="VM"/> がNullの場合でも変更を通知するか</param>
    /// <param name="fileName">出力元ファイル名</param>
    /// <param name="memberName">出力元関数名</param>
    public class SelectorViewModel<VM>(bool itemChangedIfNull = false, [CallerFilePath] string? fileName = null, [CallerMemberName] string memberName = "") :
        SelectorViewModel<VM, VM>(static vm => vm, itemChangedIfNull, fileName, memberName)
    { }
}
