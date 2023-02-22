using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SyncVariable<T> where T : IComparable<T>
{
    T mValue;
    bool mHasChanged = false;

    public void Set(T value)
    {
        if (mValue.CompareTo(value) != 0)
        {
            mHasChanged = true;
        }

        mValue = value;
    }

    public T Get()
    {
        return mValue;
    }
}
