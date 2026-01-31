using System;
using System.Collections;
using System.Linq;
using System.Reflection;

#nullable enable

namespace HouseholdAccountBook.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// 文字列化する
        /// </summary>
        /// <param name="obj">文字列化の対象</param>
        /// <param name="depth">探索の深さ</param>
        /// <returns></returns>
        public static string? ToString2(this object obj, int depth = 0)
        {
            // 再帰呼び出しの深さ制限
            depth++;
            if (10 < depth) {
                return "[overflow]";
            }

            if (obj is null) { return "null"; }

            if (obj.GetType().IsPrimitive) { return $"{obj}"; }
            if (obj.GetType().IsEnum) { return $"{obj}"; }

            switch (obj) {
                case string s:
                    return $"\"{s}\"";
                case DateTime dt:
                    return dt.TimeOfDay == TimeSpan.Zero ? $"{dt:yyyy-MM-dd}" : $"{dt:yyyy-MM-dd HH:mm:ss}";
                case IEnumerable enumerable:
                    return $"[{string.Join(", ", enumerable.Cast<object>().Select(item => item.ToString2(depth)))}]";
                default:
                    PropertyInfo[] propInfos = obj.GetType().GetProperties();
                    return string.Join(", ", propInfos.Select(propInfo => $"{propInfo.Name}:{propInfo.GetValue(obj)?.ToString2(depth) ?? "null"}"));
            }
        }
    }
}
