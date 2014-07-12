using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Common.Helpers
{
    /// <summary>
    ///     Extension methods for collections.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        ///     Execute action on each item in enumeration.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="action"></param>
        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (var item in items)
                action(item);
        }

        /// <summary>
        ///     Converts an enumerable collection to an delimited string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public static string AsDelimited<T>(this IEnumerable<T> items, string delimiter)
        {
            List<string> itemList = new List<string>();
            foreach (T item in items)
                itemList.Add(item.ToString());
            return String.Join(delimiter, itemList.ToArray());
        }

        public static bool HasDuplicates<T>(this IEnumerable<T> subjects)
        {
            return HasDuplicates(subjects, EqualityComparer<T>.Default);
        }

        public static bool HasDuplicates<T>(this IEnumerable<T> subjects, IEqualityComparer<T> comparer)
        {
            if (subjects == null)
                throw new ArgumentNullException("subjects");

            if (comparer == null)
                throw new ArgumentNullException("comparer");

            HashSet<T> set = new HashSet<T>(comparer);

            foreach (var s in subjects)
            {
                if (!set.Add(s))
                    return true;
            }

            return false;
        }

        /// <summary>
        ///     Check for any nulls.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        public static bool HasAnyNulls<T>(this IEnumerable<T> items)
        {
            return IsTrueForAny(items, t => t == null);
        }

        /// <summary>
        ///     Check if any of the items in the collection satisfied by the condition.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="executor"></param>
        /// <returns></returns>
        public static bool IsTrueForAny<T>(this IEnumerable<T> items, Func<T, bool> executor)
        {
            foreach (T item in items)
            {
                bool result = executor(item);
                if (result)
                    return true;
            }
            return false;
        }

        /// <summary>
        ///     Check if all of the items in the collection satisfied by the condition.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="executor"></param>
        /// <returns></returns>
        public static bool IsTrueForAll<T>(this IEnumerable<T> items, Func<T, bool> executor)
        {
            foreach (T item in items)
            {
                bool result = executor(item);
                if (!result)
                    return false;
            }
            return true;
        }

        /// <summary>
        ///     Check if all of the items in the collection satisfied by the condition.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public static IDictionary<T, T> ToDictionary<T>(this IList<T> items)
        {
            IDictionary<T, T> dict = new Dictionary<T, T>();
            foreach (T item in items)
                dict[item] = item;
            return dict;
        }

        public static bool ContainAnySimilarItemAs<T>(this IEnumerable<T> list1, IEnumerable<T> list2) where T : IEquatable<T>
        {
            List<T> firstList = list1.ToList();
            List<T> secondList = list2.ToList();
            return secondList.Any(item => firstList.Any(t => t.GetHashCode() == item.GetHashCode()));
        }

        public static IEnumerable<T> AsEmptyIfNull<T>(this IEnumerable<T> source)
        {
            return source ?? Enumerable.Empty<T>();
        }
    }
}