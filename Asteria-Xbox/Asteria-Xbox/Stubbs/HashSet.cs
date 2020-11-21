using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
// Copyright 2010 KD Secure, LLC
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at

//     http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

/// <summary>A simple HashSet that just extends Dictionary to add set-like behavior</summary>
/// <remarks>
/// This class is needed because HashSet is missing on Mobile.
/// </remarks>

public class HashSet<T> : IEnumerable, IEnumerable<T>
{

    public HashSet()
    {
        _dict = new Dictionary<T, bool>();
    }

    public bool Contains(T key)
    {
        if (key == null)
            return false;
        return _dict.ContainsKey(key);
    }

    public int Count
    {
        get { return _dict.Count; }
    }

    public void Clear()
    {
        _dict.Clear();
    }

    /// <summary>
    /// Add an item to the set if it is not already present
    /// </summary>
    /// <param name="key">Item to add</param>
    /// <returns>True if set already contained the given item</returns>
    /// <remarks>
    /// This functions like HashSet.add() in Java in that it does not require
    /// you to do a double-check like Dictionary's brain-dead Add() method.
    /// </remarks>
    public bool Add(T key, bool silentlyIgnoreNothing = false)
    {
        if (silentlyIgnoreNothing && key == null)
            return false;

        if (_dict.ContainsKey(key))
        {
            return true;
        }
        else
        {
            _dict.Add(key, true);
            return false;
        }
    }

    /// <summary>Remove an item from the set if present</summary>
    /// <param name="key">Item to remove</param>
    /// <returns>True if item was present, false if nothing happened</returns>
    public bool Remove(T key)
    {
        if (key == null)
            return false;
        return _dict.Remove(key);
    }

    public T[] ToArray()
    {
        T[] a = new T[_dict.Count];
        _dict.Keys.CopyTo(a, 0);
        return a;
    }

    public System.Collections.Generic.IEnumerator<T> GetEnumerator()
    {
        return _dict.Keys.GetEnumerator();
    }

    public System.Collections.IEnumerator GetEnumerator1()
    {
        return _dict.Keys.GetEnumerator();
    }
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator1();
    }


    private Dictionary<T, bool> _dict;
}

//=======================================================
//Service provided by Telerik (www.telerik.com)
//Conversion powered by NRefactory.
//Twitter: @telerik
//Facebook: facebook.com/telerik
//=======================================================
