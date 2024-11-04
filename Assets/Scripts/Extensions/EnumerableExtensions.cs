using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class EnumerableExtensions
{
    public static int GetIndex<T>(this IEnumerable<T> source, Predicate<T> predicate)
    {
        for (int i = 0; i < source.Count(); i++)
        {
            T element = source.ElementAt(i);

            if (predicate.Invoke(element))
            {
                return i;
            }
        }

        return -1;
    }

    public static T GetElementByIndex<T>(this IEnumerable<T> source, Predicate<int> predicate)
    {
        for (int i = 0; i < source.Count(); i++)
        {
            if (predicate.Invoke(i))
            {
                return source.ElementAt(i);
            }
        }

        return default;
    }
}
