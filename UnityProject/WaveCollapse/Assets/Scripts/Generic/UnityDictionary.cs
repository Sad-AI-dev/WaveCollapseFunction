using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class UnityDictionary<Key, Value> : ISerializationCallbackReceiver
{
    [System.Serializable]
    public struct Pair {
        public Key key;
        public Value value;
    }

    [SerializeField] private List<Pair> dictionary;

    private Dictionary<Key, Value> dict;

    //----------------serialization--------------------
    //dictionary to list
    public void OnBeforeSerialize() {
        //initialize values if needed
        dict ??= new Dictionary<Key, Value>();
        dictionary ??= new List<Pair>();

        TryPopulateList();
    }
    private void TryPopulateList()
    {
        if (IsValidList()) {
            dictionary.Clear();

            //populate list from dict
            foreach (KeyValuePair<Key, Value> kvp in dict) {
                dictionary.Add(new Pair { key = kvp.Key, value = kvp.Value });
            }
        }
    }
    private bool IsValidList()
    {
        Key[] keys = new Key[dictionary.Count];
        for (int i = 0; i < dictionary.Count; i++) {
            if (keys.Contains(dictionary[i].key)) { //found dupe key, list is invalid
                return false;
            }
            keys[i] = dictionary[i].key;
        }
        return true; //no dupes
    }

    //list to dictionary
    public void OnAfterDeserialize()
    {
        dict = new Dictionary<Key, Value>();
        ListToValidDictionary();
    }

    private void ListToValidDictionary()
    {
        for (int i = 0; i < dictionary.Count; i++) {
            if (!dict.ContainsKey(dictionary[i].key)) {
                dict.Add(dictionary[i].key, dictionary[i].value);
            }
        }
    }

    //-------------dictionary interfacing-------------------
    public Value this[Key key] { 
        get { return dict[key]; }
        set { dict[key] = value; }
    }

    //custom foreach support
    public IEnumerator GetEnumerator() { return dict.GetEnumerator(); }

    //---count/keys/values---
    public int Count { get { return dict.Count; } }
    public Dictionary<Key,Value>.KeyCollection Keys { get { return dict.Keys; } }
    public Dictionary<Key, Value>.ValueCollection Values { get { return dict.Values; } }

    public bool ContainsKey(Key key) { return dict.ContainsKey(key); }
    public bool ContainsValue(Value value) { return dict.ContainsValue(value); }

    //---add/remove/clear--
    public void Add(Key key, Value value) { dict.Add(key, value); }
    public void Remove(Key key) { dict.Remove(key); }
    public void Clear() { dict.Clear(); }
}