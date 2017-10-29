using System;
using System.Collections.Generic;

namespace HouseholdAccountBook.Dao
{
    /// <summary>
    /// クエリ実行結果の処理
    /// </summary>
    public partial class DaoReader
    {
        /// <summary>
        /// 実行されたSQL(デバッグ用)
        /// </summary>
        private string sql;

        /// <summary>
        /// 実行結果
        /// </summary>
        private LinkedList<Dictionary<string, object>> resultSet;
        /// <summary>
        /// エヌメレータ
        /// </summary>
        private LinkedList<Dictionary<string, object>>.Enumerator enumerator;

        /// <summary>
        /// レコード数
        /// </summary>
        public int Count { get { return this.resultSet.Count; } }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="sql">実行SQL</param>
        /// <param name="resultSet">実行結果</param>
        public DaoReader(string sql, LinkedList<Dictionary<string, object>> resultSet)
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
        /// 1レコードだけの処理
        /// </summary>
        /// <param name="record">レコード</param>
        public delegate void ExectionARow(Record record);
        /// <summary>
        /// 1レコードだけ読み込む
        /// </summary>
        /// <param name="exection">レコードの処理</param>
        public void ExecARow(ExectionARow exection)
        {
            this.enumerator.MoveNext();
            if (this.enumerator.Current != null) {
                exection(new Record(this.enumerator.Current));
            }
            else {
                throw new InvalidOperationException("有効なレコードがありません。");
            }
        }

        /// <summary>
        /// 複数行の処理
        /// </summary>
        /// <param name="count">現在の行数</param>
        /// <param name="record">レコード</param>
        /// <returns>継続の有無</returns>
        public delegate bool ExectionWholeRow(int count, Record record);
        /// <summary>
        /// 複数レコードを読み込む
        /// </summary>
        /// <param name="exection">レコードの処理</param>
        public void ExecWholeRow(ExectionWholeRow exection)
        {
            int count = 0;
            while (this.enumerator.MoveNext()) {
                if(!exection(count, new Record(this.enumerator.Current))) { break; }
                count++;
            }
            // 初期位置に戻す
            this.enumerator = this.resultSet.GetEnumerator();
        }
    }
}
