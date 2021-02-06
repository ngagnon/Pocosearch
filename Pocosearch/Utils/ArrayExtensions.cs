using System;

namespace Pocosearch.Utils
{
    public static class ArrayExtensions
    {
        public static T[] Slice<T>(this T[] array, int len)
        {
            T[] tmp = new T[len];
            Array.Copy(array, tmp, len);
            return tmp;
        }

        public static T[] Slice<T>(this T[] array, int offset, int len)
        {
            T[] tmp = new T[len];
            Array.Copy(array, offset, tmp, 0, len);
            return tmp;
        }
    }
}