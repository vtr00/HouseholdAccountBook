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
        /// <param name="value">変換対象の値</param>
        /// <param name="defaultValue">範囲外の場合の値</param>
        public static T SafeParseEnum<T>(int value, T defaultValue) where T : struct, Enum => Enum.IsDefined(typeof(T), value) ? (T)Enum.ToObject(typeof(T), value) : defaultValue;
    }
}
