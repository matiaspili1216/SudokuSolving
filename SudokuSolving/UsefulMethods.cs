using System;
using System.Collections.Generic;
using System.Linq;

namespace SudokuSolving
{
    public static class UsefulMethods
    {
        public static bool AllElementAreEquals<T>(this IEnumerable<T> elements, out T elementsEqual)
        {
            if (elements is null || !elements.Any()) { throw new ArgumentNullException(nameof(elements)); }

            if (elements.Count() == 1) { elementsEqual = elements.FirstOrDefault(); return true; }
            else
            {
                var firstElement = elements.FirstOrDefault();
                var result = elements.All(x => x.Equals(firstElement));
                elementsEqual = result ? firstElement : default;
                return result;
            }
        }

        public static bool In<T>(this T t, IEnumerable<T> elements) => elements.Contains(t);

        public static List<string> Split(this List<string> list, int chunkSize)
        {

            var result = new List<string>();

            for (int i = 0; i < list.Count; i += chunkSize)
            {
                result.Add(string.Join("|", list.GetRange(i, Math.Min(chunkSize, list.Count - i))));
            }

            return result;
        }
    }
}
