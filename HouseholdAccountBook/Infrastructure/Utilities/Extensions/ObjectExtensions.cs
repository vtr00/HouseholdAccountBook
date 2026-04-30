using HouseholdAccountBook.Infrastructure.Utilities.Attributes;
using HouseholdAccountBook.Models.ValueObjects;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

#nullable enable

namespace HouseholdAccountBook.Infrastructure.Utilities.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// 文字列化する
        /// </summary>
        /// <param name="obj">文字列化の対象</param>
        /// <param name="depth">探索の深さ</param>
        /// <returns>文字列</returns>
        public static string? ToString2(this object obj, int depth = 0)
        {
            // 再帰呼び出しの深さ制限
            depth++;
            if (10 < depth) {
                return "[overflow]";
            }

            if (obj is null) { return "null"; }
            if (obj is Delegate) { return "delegate"; }

            if (obj.GetType().IsPrimitive) { return $"{obj}"; }
            if (obj.GetType().IsEnum) { return $"{obj}"; }

            switch (obj) {
                case string s:
                    return $"\"{s.Replace(Environment.NewLine, "\\r\\n")}\"";
                case decimal decim:
                    return $"{decim}";
                case DateOnly d:
                    return $"{d:yyyy-MM-dd}";
                case TimeOnly t:
                    return $"{t:HH:mm:ss}";
                case DateTime dt:
                    return dt.TimeOfDay == TimeSpan.Zero ? $"{dt:yyyy-MM-dd}" : $"{dt:yyyy-MM-dd HH:mm:ss}";
                case IEnumerable enumerable:
                    return $"[{string.Join(", ", enumerable.Cast<object>().Select(item => item.ToString2(depth)))}]";
                case IIdObj idObj:
                    return $"{idObj.Id}";
                case Encoding encoding:
                    return $"{encoding.EncodingName}";
                case ITuple tuple when obj.GetType().FullName?.StartsWith("System.ValueTuple`") == true: {
                    FieldInfo[] fieldInfos = obj.GetType().GetFields(); // タプルが保持しているフィールドの情報を取得
                    string tmp = string.Join(", ", fieldInfos.Select(fieldInfo => $"{fieldInfo.GetValue(obj)?.ToString2(depth) ?? "null"}"));
                    return $"({tmp})";
                }
                default: {
                    PropertyInfo[] propInfos = obj.GetType().GetProperties(); // クラスが保持しているプロパティの情報を取得
                    string tmp = string.Join(", ", propInfos.Select(propInfo => {
                        IgnoreToStringAttribute? attr = propInfo.GetCustomAttribute<IgnoreToStringAttribute>(); // この属性が付与されているプロパティは出力しない
                        return attr != null ? $"{propInfo.Name}:{attr.Alternative}" : $"{propInfo.Name}:{propInfo.GetValue(obj)?.ToString2(depth) ?? "null"}";
                    }));
                    return depth == 1 ? tmp : $"[{tmp}]";
                }
            }
        }
    }
}
