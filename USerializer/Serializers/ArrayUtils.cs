using System;

namespace USerialization
{
    public class ArrayUtils
    {
        public static void CheckExpand<T>(ref T[] array, ref int size)
        {
            if (size >= array.Length)
            {
                var expanded = new T[size * 2];
                Array.Copy(array, expanded, array.Length);
                array = expanded;
            }
        }

        public static void Insert<T>(ref T[] array, ref int size, int index, T value)
        {
            CheckExpand(ref array, ref size);

            if (index < size)
                Array.Copy(array, index, array, index + 1, size - index);

            array[index] = value;
            size++;
        }


        public static void Add<T>(ref T[] array, ref int size, T value)
        {
            CheckExpand(ref array, ref size);
            array[size] = value;
            size++;
        }

        public static void CropToSize<T>(ref T[] array, ref int size)
        {
            if (size >= array.Length)
                return;

            var cropped = new T[size];
            Array.Copy(array, cropped, size);
            array = cropped;
        }
    }
}