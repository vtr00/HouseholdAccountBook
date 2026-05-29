using System;

namespace HouseholdAccountBook.Infrastructure.Utilities
{
    /// <summary>
    /// Enumユーティリティ
    /// </summary>
    public static class EnumUtil
    {
        /// <summary>
        /// 数値を安全にEnumに変換し、範囲外なら指定値を返す
        /// </summary>
        /// <typeparam name="T">変換対象の型</typeparam>
        /// <param name="value">変換対象の値</param>
        /// <param name="defaultValue">範囲外の場合の値</param>
        /// <returns>変換後の値</returns>
        public static T SafeCastEnum<T>(int value, T defaultValue) where T : struct, Enum => Enum.IsDefined(typeof(T), value) ? (T)Enum.ToObject(typeof(T), value) : defaultValue;

        /// <summary>
        /// 指定されたオブジェクトを安全にEnumに変換し、範囲外なら指定値を返す
        /// </summary>
        /// <typeparam name="T">変換対象の型</typeparam>
        /// <param name="obj">変換対象の値</param>
        /// <param name="defaultValue">範囲外の場合の値</param>
        /// <returns>変換後の値</returns>
        public static T SafeCastEnum<T>(object obj, T defaultValue) where T : struct, Enum
        {
            if (obj is not null) {
                if (obj is T t) {
                    return t;
                }
                else if (obj is int i) {
                    return SafeCastEnum(i, defaultValue);
                }
            }
            return defaultValue;
        }
    }
}
