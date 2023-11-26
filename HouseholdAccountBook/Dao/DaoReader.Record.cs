using System;
using System.Collections.Generic;

namespace HouseholdAccountBook.Dao
{
    public partial class DaoReader
    {
        /// <summary>
        /// 取得レコード
        /// </summary>
        public class Record
        {
            /// <summary>
            /// レコード
            /// </summary>
            private readonly Dictionary<string, object> _record;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="record">レコード</param>
            public Record(Dictionary<string, object> record)
            {
                this._record = record;
            }

            /// <summary>
            /// string型で取得する
            /// </summary>
            /// <param name="key">カラム名</param>
            /// <returns></returns>
            public string this[string key]
            {
                get {
                    key = key.ToLower();
                    if (this._record.ContainsKey(key)) {
                        return this._record[key].ToString();
                    }
                    else {
                        throw new KeyNotFoundException();
                    }
                }
            }

            /// <summary>
            /// Int型で取得する
            /// </summary>
            /// <param name="key">カラム名</param>
            /// <returns></returns>
            public int ToInt(string key)
            {
                key = key.ToLower();
                if (this._record.ContainsKey(key)) {
                    object tmp = this._record[key];
                    return Int32.Parse(tmp.ToString());
                }
                else {
                    throw new KeyNotFoundException();
                }
            }

            /// <summary>
            /// Int?型で取得する
            /// </summary>
            /// <param name="key">カラム名</param>
            /// <returns></returns>
            public int? ToNullableInt(string key)
            {
                key = key.ToLower();
                if (this._record.ContainsKey(key)) {
                    object tmp = this._record[key];
                    return tmp == DBNull.Value ? null : (int?)tmp;
                }
                else {
                    throw new KeyNotFoundException();
                }
            }

            /// <summary>
            /// bool型で取得する
            /// </summary>
            /// <param name="key">カラム名</param>
            /// <returns></returns>
            public bool ToBoolean(string key)
            {
                key = key.ToLower();
                if (this._record.ContainsKey(key)) {
                    try {
                        return (bool)this._record[key];
                    }
                    catch (InvalidCastException) {
                        string tmp = this._record[key] as string;
                        return tmp == "True" || tmp == "true";
                    }
                }
                else {
                    throw new KeyNotFoundException();
                }
            }

            /// <summary>
            /// DateTime型で取得する
            /// </summary>
            /// <param name="key">カラム名</param>
            /// <returns></returns>
            public DateTime ToDateTime(string key)
            {
                key = key.ToLower();
                if (this._record.ContainsKey(key)) {
                    object tmp = this._record[key];
                    return DateTime.Parse(tmp.ToString());
                }
                else {
                    throw new KeyNotFoundException();
                }
            }

            /// <summary>
            /// DateTime?型で取得する
            /// </summary>
            /// <param name="key">カラム名</param>
            /// <returns></returns>
            public DateTime? ToNullableDateTime(string key)
            {
                key = key.ToLower();
                if (this._record.ContainsKey(key)) {
                    object tmp = this._record[key];
                    if (tmp == DBNull.Value) return null;
                    return DateTime.Parse(tmp.ToString());
                }
                else {
                    throw new KeyNotFoundException();
                }
            }
        }
    }
}
