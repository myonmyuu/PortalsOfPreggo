using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StuffLib
{
    public static T GetRandom<T>(this IList<T> list)
    {
        return list[Random.Range(0, list.Count)];
    }

    public static int Wrap(this int value, int min, int max)
    {
        int range = max - min + 1;
        int wrappedValue = ((value - min) % range + range) % range + min;
        return wrappedValue;
    }
}
