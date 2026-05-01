using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.Utilities.Args;
using HouseholdAccountBook.Infrastructure.Utilities.Extensions;
using HouseholdAccountBook.Models.AppServices;
using HouseholdAccountBook.ViewModels.Abstract;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

#nullable enable

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// <see cref="SelectorViewModel{VM, KEY}"/> で読み込み時に KEY を選択するモード
    /// </summary>
    public enum SelectorMode
    {
        // 一致する項目がなければデフォルト(KEYがクラスなら選択なし状態)を選択する
        FirstOrDefault,
        // 一致する項目がなければ先頭の項目を選択する
        FirstOrElementAtOrDefault,
        // 一致する項目がなくとも選択する
        Force
    }

    /// <summary>
    /// VMリストとその選択VM(リスト)を保持するVM
    /// </summary>
    /// <typeparam name="VM">ViewModel</typeparam>
    /// <typeparam name="KEY">ViewModelを特定するキー(IDなど)</typeparam>
    public class SelectorViewModel<VM, KEY> : BindableBase, ILoadableAsync, IDisposable
    {
        #region フィールド
        /// <summary>
        /// <see cref="VM"/> から <see cref="KEY"/> への変換処理
        /// </summary>
        private readonly Func<VM?, KEY?> mSelector;
        /// <summary>
        /// 処理中状態サービス
        /// </summary>
        private readonly BusyService? mBusyService;
        /// <summary>
        /// 選択 <see cref="VM"/> がNullの場合でも変更を通知するか
        /// </summary>
        private readonly bool mItemChangedIfNull;

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
        /// 読み込み時の選択モード
        /// </summary>
        private SelectorMode mMode;
        /// <summary>
        /// 読込処理中か
        /// </summary>
        private bool mOnLoad = false;

        /// <summary>
        /// 多重読込処理防止トークン源
        /// </summary>
        private CancellationTokenSource? mSelectionCts;

        /// <summary>
        /// 出力元ファイル名
        /// </summary>
        private readonly string? mFileName;
        /// <summary>
        /// 出力元メンバー名
        /// </summary>
        private readonly string mMemberName;
        #endregion

        #region イベント
        /// <summary>
        /// <see cref="VM"/> リスト変更時イベント
        /// </summary>
        public event NotifyCollectionChangedEventHandler? CollectionChanged;
        /// <summary>
        /// 選択 <see cref="VM"> 変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<KEY>>? SelectionChanged;
        /// <summary>
        /// 選択 <see cref="VM"/> リスト変更時イベント
        /// </summary>
        public event NotifyCollectionChangedEventHandler? SelectedCollectionChanged;
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
            get => this.mSelectedItem;
            set {
                KEY? old = this.mSelectedKey;
                if (this.SetProperty(ref this.mSelectedItem, value)) {
                    this.mSelectedKey = this.mSelector(value);

                    // 読込処理中以外か
                    if (!this.mOnLoad) {
                        // NULLの場合に通知するか
                        if (this.mSelectedKey != null || this.mItemChangedIfNull) {
                            // 待機せず選択時の処理をする
                            this.OnSelectionChangedAsync(old, this.mSelectedKey).FireAndForget();
                        }
                    }

                    this.RaisePropertyChanged(nameof(this.SelectedKey));
                    this.RaisePropertyChanged(nameof(this.SelectedIndex));
                }
            }
        }
        /// <summary>
        /// 選択 <see cref="VM"/> 内部値
        /// </summary>
        private VM? mSelectedItem = default;
        /// <summary>
        /// 選択 <see cref="KEY"/>
        /// </summary>
        /// <remarks>編集可能な <see cref="ComboBox"/> では <see cref="ComboBox.Text"/> とこのプロパティを Bind する</remarks>
        public KEY? SelectedKey {
            get => this.mSelectedKey;
            set {
                KEY? old = this.mSelectedKey;
                if (this.SetProperty(ref this.mSelectedKey, value)) {
                    // KEYの一致するVMが存在するならSelectedItemに設定する。なければdefaultにしてfieldに入力値を保持する
                    this.mSelectedItem = this.ItemList.FirstOrDefault(vm => KeyEquals(this.mSelector(vm), value));

                    // 読込処理中以外か
                    if (!this.mOnLoad) {
                        // NULLの場合に通知するか
                        if (this.mSelectedKey != null || this.mItemChangedIfNull) {
                            // 待機せず選択時の処理をする
                            this.OnSelectionChangedAsync(old, this.mSelectedKey).FireAndForget();
                        }
                    }

                    this.RaisePropertyChanged(nameof(this.SelectedItem));
                    this.RaisePropertyChanged(nameof(this.SelectedIndex));
                }
            }
        }
        /// <summary>
        /// 選択 <see cref="KEY"/> 内部値
        /// </summary>
        private KEY? mSelectedKey = default;
        /// <summary>
        /// 選択インデックス
        /// </summary>
        public int SelectedIndex {
            get => this.SelectedItem == null ? -1 : this.ItemList.IndexOf(this.SelectedItem);
            set => this.SelectedItem = (value < 0 || this.ItemList.Count <= value) ? default : this.ItemList[value];
        }

        /// <summary>
        /// 選択 <see cref="VM"/> リスト
        /// </summary>
        /// <remarks>BehaviorにBindするには<see cref="VM"> が <see cref="ISelectable"/> を継承している必要がある</remarks>
        public ObservableCollection<VM>? SelectedItemList {
            get;
            set {
                if (value != null) {
                    IEnumerable<VM> added = [.. value.Except(field ?? [])];
                    IEnumerable<VM> removed = [.. field?.Except(value) ?? []];

                    foreach (VM vm in added) {
                        field?.Add(vm);
                    }
                    foreach (VM vm in removed) {
                        _ = field?.Remove(vm);
                    }
                }
                else {
                    // null の場合はリストを空にする(ClearだとBehaviorが意図した挙動にならない)
                    while (0 < (field?.Count ?? 0)) {
                        field?.RemoveAt(0);
                    }
                }
            }
        } = [];
        /// <summary>
        /// 選択 <see cref="VM"/> リストのアイテム数
        /// </summary>
        public int SelectedCount => this.SelectedItemList?.Count ?? 0;
        /// <summary>
        /// 選択 <see cref="KEY"/> リスト
        /// </summary>
        public IEnumerable<KEY?> SelectedKeys {
            get => this.SelectedItemList?.Select(vm => this.mSelector(vm)) ?? [];
            set => throw new NotImplementedException();
        }
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="selector"><see cref="VM"/> から <see cref="KEY"/> への変換処理</param>
        /// <param name="busyService">処理中状態サービス</param>
        /// <param name="itemChangedIfNull">選択 <see cref="VM"/> がNullの場合でも変更を通知するか</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="memberName">出力元関数名</param>
        public SelectorViewModel(Func<VM?, KEY?> selector, BusyService? busyService = null, bool itemChangedIfNull = false,
                                 [CallerFilePath] string? fileName = null, [CallerMemberName] string memberName = "")
        {
            using FuncLog funcLog = new(level: Log.LogLevel.Trace, fileName: fileName, methodName: memberName);

            this.mSelector = selector;
            this.mBusyService = busyService;
            this.mItemChangedIfNull = itemChangedIfNull;
            this.mFileName = fileName;
            this.mMemberName = memberName;

            this.ItemList.CollectionChanged += (sender, e) => {
                if (!this.mOnLoad) {
                    using FuncLog funcLog = new(level:Log.LogLevel.Debug, fileName: this.mFileName, methodName: $"{this.mMemberName}.{nameof(this.CollectionChanged)}");
                }

                this.RaisePropertyChanged(nameof(this.Count));
                this.CollectionChanged?.Invoke(sender, e);
            };
            this.SelectedItemList.CollectionChanged += (sender, e) => {
                if (!this.mOnLoad) {
                    using FuncLog funcLog = new(level:Log.LogLevel.Debug, fileName: this.mFileName, methodName: $"{this.mMemberName}.{nameof(this.SelectedCollectionChanged)}");
                }

                this.RaisePropertyChanged(nameof(this.SelectedCount));
                this.SelectedCollectionChanged?.Invoke(sender, e);
            };
        }

        ~SelectorViewModel()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            this.mSelectionCts?.Cancel();
            this.mSelectionCts?.Dispose();
            GC.SuppressFinalize(this);
        }

        private static readonly EqualityComparer<KEY?> mComparer = EqualityComparer<KEY?>.Default;
        /// <summary>
        /// 2 つのキー値が等しいかどうかを判定する
        /// </summary>
        /// <remarks>このメソッドは型パラメーター KEY の既定の等値比較子を使用して比較を行う</remarks>
        /// <param name="key1">比較する最初のキー値</param>
        /// <param name="key2">比較する 2 番目のキー値</param>
        /// <returns>キー値が等しい場合は <see langword="true"/>。それ以外の場合は <see langword="false"/></returns>
        private static bool KeyEquals(KEY? key1, KEY? key2) => mComparer.Equals(key1, key2);

        #region SetLoader
        /// <summary>
        /// <see cref="VM"> リスト読込処理を設定する
        /// </summary>
        /// <param name="load">[同期] <see cref="VM"/> リスト読込処理</param>
        /// <param name="canLoad">Load可能判定処理</param>
        /// <param name="mode">読み込み時に <see cref="KEY"/> を選択するモード</param>
        public void SetLoader(Func<IEnumerable<VM>> load, Func<bool>? canLoad = null, SelectorMode mode = SelectorMode.FirstOrElementAtOrDefault) =>
            this.SetLoader(funcLog => load(), canLoad, mode);
        /// <summary>
        /// <see cref="VM"> リスト読込処理を設定する
        /// </summary>
        /// <param name="load">[同期] <see cref="VM"/> リスト読込処理</param>
        /// <param name="canLoad">Load可能判定処理</param>
        /// <param name="mode">読み込み時に <see cref="KEY"/> を選択するモード</param>
        public void SetLoader(Func<FuncLog, IEnumerable<VM>> load, Func<bool>? canLoad = null, SelectorMode mode = SelectorMode.FirstOrElementAtOrDefault)
        {
            using FuncLog funcLog = new(fileName: this.mFileName, methodName: $"{this.mMemberName}.{nameof(SetLoader)}");

            this.mCanLoad = canLoad;
            this.mLoad = load;
            this.mLoadAsync = null;
            this.mMode = mode;
        }

        /// <summary>
        /// <see cref="VM"> リスト読込処理を設定する
        /// </summary>
        /// <param name="loadAsync">[非同期] <see cref="VM"> リスト読込処理</param>
        /// <param name="canLoad">Load可能判定処理</param>
        /// <param name="mode">読み込み時に <see cref="KEY"/> を選択するモード</param>
        public void SetLoader(Func<Task<IEnumerable<VM>>> loadAsync, Func<bool>? canLoad = null, SelectorMode mode = SelectorMode.FirstOrElementAtOrDefault) =>
            this.SetLoader((funcLog, token) => loadAsync(), canLoad, mode);
        /// <summary>
        /// <see cref="VM"> リスト読込処理を設定する
        /// </summary>
        /// <param name="loadAsync">[非同期] <see cref="VM"> リスト読込処理</param>
        /// <param name="canLoad">Load可能判定処理</param>
        /// <param name="mode">読み込み時に <see cref="KEY"/> を選択するモード</param>
        public void SetLoader(Func<CancellationToken, Task<IEnumerable<VM>>> loadAsync, Func<bool>? canLoad = null, SelectorMode mode = SelectorMode.FirstOrElementAtOrDefault) =>
            this.SetLoader((funcLog, token) => loadAsync(token), canLoad, mode);
        /// <summary>
        /// <see cref="VM"> リスト読込処理を設定する
        /// </summary>
        /// <param name="loadAsync">[非同期] <see cref="VM"> リスト読込処理</param>
        /// <param name="canLoad">Load可能判定処理</param>
        /// <param name="mode">読み込み時に <see cref="KEY"/> を選択するモード</param>
        public void SetLoader(Func<FuncLog, CancellationToken, Task<IEnumerable<VM>>> loadAsync, Func<bool>? canLoad = null, SelectorMode mode = SelectorMode.FirstOrElementAtOrDefault)
        {
            using FuncLog funcLog = new(fileName: this.mFileName, methodName: $"{this.mMemberName}.{nameof(SetLoader)}");

            this.mCanLoad = canLoad;
            this.mLoad = null;
            this.mLoadAsync = loadAsync;
            this.mMode = mode;
        }
        #endregion

        /// <summary>
        /// 読込み可能か
        /// </summary>
        /// <returns>読込み可能/不可能</returns>
        private bool CanLoad() => this.mCanLoad?.Invoke() ?? true;

        /// <summary>
        /// [非同期] <see cref="VM"/> リストを読込み、現在選択中の <see cref="VM"/> の選択を維持する
        /// </summary>
        /// <param name="token">キャンセル用トークン</param>
        /// <returns></returns>
        public async Task LoadAsync(CancellationToken token = default) => await this.LoadAsyncCore(default, token);
        /// <summary>
        /// [非同期] <see cref="VM"/> リストを読込み、指定の <see cref="VM"/> を選択する
        /// </summary>
        /// <param name="selection"><see cref="VM"/> を選択するキー. nullの場合は現在選択中の <see cref="VM"/> の選択を維持する</param>
        /// <param name="token">キャンセル用トークン</param>
        /// <returns></returns>
        public async Task LoadAsync(KEY? selection, CancellationToken token = default) => await this.LoadAsyncCore([selection], token);
        /// <summary>
        /// [非同期] <see cref="VM"/> リストを読込み、指定の <see cref="VM"/> を選択する
        /// </summary>
        /// <param name="selections"><see cref="VM"/> を選択するキー. nullの場合は現在選択中の <see cref="VM"/> の選択を維持する</param>
        /// <param name="token">キャンセル用トークン</param>
        /// <returns></returns>
        public async Task LoadAsync(IEnumerable<KEY?>? selections, CancellationToken token = default) => await this.LoadAsyncCore(selections, token);
        /// <summary>
        /// [非同期] <see cref="VM"/> リストを読込み、指定の <see cref="VM"/> を選択する
        /// </summary>
        /// <param name="selections"><see cref="VM"/> を選択するキーのリスト. nullの場合は現在選択中の <see cref="VM"/> の選択を維持する</param>
        /// <param name="token">キャンセル用トークン</param>
        /// <returns></returns>
        protected async Task LoadAsyncCore(IEnumerable<KEY?>? selections, CancellationToken token = default)
        {
            using FuncLog funcLog = new(new { selections }, fileName: this.mFileName, methodName: $"{this.mMemberName}.{nameof(LoadAsyncCore)}");

            if (this.mLoad is null && this.mLoadAsync is null) { throw new InvalidOperationException(); }
            if (this.mLoad is not null && this.mLoadAsync is not null) { throw new InvalidOperationException(); }

            if (!this.CanLoad()) {
                Log.Debug($"{nameof(this.CanLoad)} is false");
                return;
            }

            using IDisposable? disposable = this.mBusyService?.Enter();

            this.mOnLoad = true;
            try {
                // リスト読込前の値を取得する
                KEY? oldKey = this.SelectedKey;
                IEnumerable<KEY?> oldKeys = this.SelectedKeys;

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

                // 一旦未選択状態にする(ClearだとBehaviorが意図した挙動にならないのでRemoveAtで空にする)
                this.SelectedItem = default;
                this.SelectedItemList = default;

                // 取得したVMを反映する
                this.ItemList.Clear();
                foreach (VM tmp in tmpList) {
                    this.ItemList.Add(tmp);
                }

                // 先頭の一つを選択する
                KEY? tmpSelection = selections != null ? selections.FirstOrDefault() : oldKey;
                switch (this.mMode) {
                    case SelectorMode.FirstOrElementAtOrDefault:
                        this.SelectedItem = this.ItemList.FirstOrElementAtOrDefault(vm => KeyEquals(this.mSelector(vm), tmpSelection), 0);
                        break;
                    case SelectorMode.FirstOrDefault:
                        this.SelectedItem = this.ItemList.FirstOrDefault(vm => KeyEquals(this.mSelector(vm), tmpSelection));
                        break;
                    case SelectorMode.Force:
                        this.SelectedKey = tmpSelection;
                        break;
                    default:
                        throw new NotSupportedException();
                }
                // 全部を選択する
                IList<KEY?> newKeys = [.. selections ?? oldKeys];
                this.SelectedItemList = [.. this.ItemList.Where(vm => newKeys.Contains(this.mSelector(vm), mComparer))];

                if (!KeyEquals(this.SelectedKey, oldKey)) {
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
            catch (OperationCanceledException) {
                Log.Debug($"{this.mMemberName}.{nameof(LoadAsync)} Canceled.");
                if (token != default) {
                    throw;
                }
            }
            finally {
                this.mOnLoad = false;
            }
        }

        /// <summary>
        /// <see cref="VM"/> リストの選択変更時処理
        /// </summary>
        /// <param name="oldKey">前回の選択 <see cref="KEY"/></param>
        /// <param name="newKey">今回の選択 <see cref="KEY"/></param>
        /// <param name="token">キャンセル用トークン</param>
        protected async Task OnSelectionChangedAsync(KEY? oldKey, KEY? newKey, CancellationToken token = default)
        {
            ChangedEventArgs<KEY> args = new() { OldValue = oldKey, NewValue = newKey };
            using FuncLog funcLog = new(args, fileName: this.mFileName, methodName: $"{this.mMemberName}.{nameof(OnSelectionChangedAsync)}");

            // 変更時に外部に作用する要素がなければ処理しない(BusyService.Enterを防ぐ)
            if (this.SelectionChanged is null && this.Children.Count is 0) {
                Log.Debug("Nothing to do.");
                return;
            }

            using IDisposable? disposable = this.mBusyService?.Enter();

            this.SelectionChanged?.Invoke(this, args);

            if (this.Children.Count != 0) {
                CancellationToken tmpToken = token;
                if (tmpToken == default) {
                    this.mSelectionCts?.Cancel();
                    this.mSelectionCts?.Dispose();
                    this.mSelectionCts = new();
                    tmpToken = this.mSelectionCts.Token;
                }

                try {
                    Task[] tasks = [.. this.Children.Select(async c => {
                    tmpToken.ThrowIfCancellationRequested();
                    await c.LoadAsync(tmpToken);
                    tmpToken.ThrowIfCancellationRequested();
                })];
                    await Task.WhenAll(tasks);
                }
                catch (OperationCanceledException) {
                    Log.Debug($"{this.mMemberName}.{nameof(OnSelectionChangedAsync)} Canceled.");
                    // キャンセル用トークンが引数で与えられていた場合は再スロー
                    if (token != default) {
                        throw;
                    }
                }
            }
        }
    }

    /// <summary>
    /// VMリストとその選択VM(リスト)を保持するVM
    /// </summary>
    /// <param name="busyService">処理中状態サービス</param>
    /// <param name="itemChangedIfNull">選択 <see cref="VM"/> がNullの場合でも変更を通知するか</param>
    /// <param name="fileName">出力元ファイル名</param>
    /// <param name="memberName">出力元関数名</param>
    public class SelectorViewModel<VM>(BusyService? busyService = null, bool itemChangedIfNull = false, [CallerFilePath] string? fileName = null, [CallerMemberName] string memberName = "") :
        SelectorViewModel<VM, VM>(static vm => vm, busyService, itemChangedIfNull, fileName, memberName)
    { }
}
