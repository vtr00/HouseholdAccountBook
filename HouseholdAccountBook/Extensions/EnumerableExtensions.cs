using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace HouseholdAccountBook.Extensions
{
    public static class EnumerableExtensions
    {
        public static TSource? FirstOrElementAtOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, Index index)
        {
            TSource? tmp = source.FirstOrDefault(predicate);
            tmp ??= source.ElementAtOrDefault(index);
            return tmp;
        }

        public static TSource? FirstOrElementAtOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, int index)
        {
            TSource? tmp = source.FirstOrDefault(predicate);
            tmp ??= source.ElementAtOrDefault(index);
            return tmp;
        }

        public static TSource? FirstOrElementAtOrDefault<TSource>(this IEnumerable<TSource> source, Index index)
            => source.FirstOrDefault(source.ElementAtOrDefault(index));

        public static TSource? FirstOrElementAtOrDefault<TSource>(this IEnumerable<TSource> source, int index)
            => source.FirstOrDefault(source.ElementAtOrDefault(index));
    }
}
