using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public static class Extensions
{
    public static ref T GetRef<T>(this NativeArray<T> array, int index) where T : struct
    {
        if (index < 0 || index >= array.Length)
            throw new ArgumentOutOfRangeException(nameof(index));
        unsafe
        {
            return ref UnsafeUtility.ArrayElementAsRef<T>(array.GetUnsafePtr(), index);
        }
    }
}