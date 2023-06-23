using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UnityDictionary<TKey, TValue> : ISerializationCallbackReceiver
{
    [System.Serializable]
    public struct Pair {
        public TKey key;
        public TValue value;
    }

    [SerializeField] private List<Pair> dictionary;

    private Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>();

    //----------------serialization--------------------
    //dictionary to lists
    public void OnBeforeSerialize() {
        if (dict != null && ListIsValid()) {
            dictionary.Clear();
            //populate list
            foreach (var kvp in dict) {
                dictionary.Add(new Pair { key = kvp.Key, value = kvp.Value });
            }
        }
    }

    //lists to dictionary
    public void OnAfterDeserialize()
    {
        dict = new Dictionary<TKey, TValue>();

        for (int i = 0; i < dictionary.Count; i++) {
            dict.Add(dictionary[i].key, dictionary[i].value);
        }
    }

    //-----------valid lists check------------
    private bool ListIsValid()
    {
        List<TKey> keys = new List<TKey>();
        foreach (Pair pair in dictionary) {
            if (keys.Contains(pair.key)) {
                return false; //duplicate key found
            }
            keys.Add(pair.key); //register key
        }
        return true; //no duplicate keys found
    }

    //-------------dictionary interfacing-------------------
    public TValue this[TKey key] { 
        get { return dict[key]; }
        set { dict[key] = value; }
    }

    //custom foreach support
    public IEnumerator GetEnumerator() { return dict.GetEnumerator(); }

    //---count/keys/values---
    public int Count { get { return dict.Count; } }
    public Dictionary<TKey,TValue>.KeyCollection Keys { get { return dict.Keys; } }
    public Dictionary<TKey, TValue>.ValueCollection Values { get { return dict.Values; } }

    public bool ContainsKey(TKey key) { return dict.ContainsKey(key); }
    public bool ContainsValue(TValue value) { return dict.ContainsValue(value); }

    //---add/remove/clear--
    public void Add(TKey key, TValue value) { dict.Add(key, value); }
    public void Remove(TKey key) { dict.Remove(key); }
    public void Clear() { dict.Clear(); }
}