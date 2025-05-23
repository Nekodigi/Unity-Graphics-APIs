using System.Linq;
using UnityEngine;

namespace Common.DebugUtils
{
    public static class Extensions
    {
        /// <summary>
        /// Prints the contents of an array to the console. Print all if less than or equal <c>elementNumLimit</c>, or print first <c>characterLimit</c> characters.
        /// </summary>
        public static void Log<T>(this T[] array, string name, int elementNumLimit = 100)
        {
            var outputStr = $"{name}: ";
            var limitedArray = array.Take(elementNumLimit).ToArray();
            outputStr += limitedArray.Select(item => item.ToString())
                .Aggregate((current, item) => current + (", " + item));
            Debug.Log(outputStr);
        }
    }
}