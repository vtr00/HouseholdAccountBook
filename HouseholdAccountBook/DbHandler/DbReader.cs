using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.DbHandler
{
    /// <summary>
    /// クエリ実行結果の処理
    /// </summary>
    public partial class DbReader
    {
        /// <summary>
        /// 実行されたSQL(デバッグ用)
        /// </summary>
        private readonly string sql;

        /// <summary>
        /// 実行結果
        /// </summary>
        private readonly LinkedList<Dictionary<string, object>> resultSet;
        /// <summary>
        /// エヌメレータ
        /// </summary>
        private LinkedList<Dictionary<string, object>>.Enumerator enumerator;

        /// <summary>
        /// レコード数
        /// </summary>
        public int Count => this.resultSet.Count;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="sql">実行SQL</param>
        /// <param name="resultSet">実行結果</param>
        public DbReader(string sql, LinkedList<Dictionary<string, object>> resultSet)
        {
            this.sql = sql;
            this.resultSet = resultSet;
            this.enumerator = resultSet.GetEnumerator();
        }

        /// <summary>
        /// 実行結果の指定位置のレコードを取得する
        /// </summary>
        /// <param name="index">指定位置</param>
        /// <returns>レコード</returns>
        public Record this[int index]
        {
            get {
                if (index < 0 || this.Count <= index) {
                    throw new IndexOutOfRangeException();
                }

                LinkedListNode<Dictionary<string, object>> node;
                if (index < this.Count / 2) { // 前半の場合
                    node = this.resultSet.First;
                    for (int i = 0; i != index; ++i) {
                        node = node.Next;
                    }
                }
                else { // 後半の場合
                    node = this.resultSet.Last;
                    for (int i = this.Count - 1; i != index; --i) {
                        node = node.Previous;
                    }
                }

                return new Record(node.Value);
            }
            private set {; }
        }

        /// <summary>
        /// [同期]1レコードだけの処理
        /// </summary>
        /// <param name="record">レコード</param>
        public delegate void ExectionARow(Record record);

        /// <summary>
        /// [同期]1レコードだけ読み込む
        /// </summary>
        /// <param name="exection">レコードの処理</param>
        public void ExecARow(ExectionARow exection)
        {
            this.enumerator = this.resultSet.GetEnumerator();
            if (this.enumerator.MoveNext()) {
                exection(new Record(this.enumerator.Current));
            }
            else {
                throw new InvalidOperationException("有効なレコードがありません。");
            }
        }

        /// <summary>
        /// [非同期]1レコードだけの処理
        /// </summary>
        /// <param name="record">レコード</param>
        public delegate Task ExectionARowAsync(Record record);

        /// <summary>
        /// [非同期]1レコードだけ読み込む
        /// </summary>
        /// <param name="exection">レコードの処理</param>
        public async Task ExecARowAsync(ExectionARowAsync exection)
        {
            this.enumerator = this.resultSet.GetEnumerator();
            if (this.enumerator.MoveNext()) {
                await exection(new Record(this.enumerator.Current));
            }
            else {
                throw new InvalidOperationException("有効なレコードがありません。");
            }
        }

        /// <summary>
        /// [同期]複数行の処理
        /// </summary>
        /// <param name="count">現在の行数</param>
        /// <param name="record">レコード</param>
        /// <returns>継続の有無</returns>
        public delegate bool ExectionWholeRow(int count, Record record);

        /// <summary>
        /// [同期]複数レコードを読み込む
        /// </summary>
        /// <param name="exection">レコードの処理</param>
        public void ExecWholeRow(ExectionWholeRow exection)
        {
            int count = 0;

            // 初期位置に戻す
            this.enumerator = this.resultSet.GetEnumerator();
            try {
                while (this.enumerator.MoveNext()) {
                    if (!exection(count, new Record(this.enumerator.Current))) { break; }

                    count++;
                }
            }
            catch (Exception) {
                ;
            }
        }

        /// <summary>
        /// [非同期]複数行の処理
        /// </summary>
        /// <param name="count">現在の行数</param>
        /// <param name="record">レコード</param>
        /// <returns>継続の有無</returns>
        public delegate Task<bool> ExectionWholeRowAsync(int count, Record record);

        /// <summary>
        /// [非同期]複数レコードを読み込む
        /// </summary>
        /// <param name="exection">レコードの処理</param>
        public async Task ExecWholeRowAsync(ExectionWholeRowAsync exection)
        {
            int count = 0;

            // 初期位置に戻す
            this.enumerator = this.resultSet.GetEnumerator();
            while (this.enumerator.MoveNext()) {
                if (!await exection(count, new Record(this.enumerator.Current))) { break; }

                count++;
            }
        }
    }
}
