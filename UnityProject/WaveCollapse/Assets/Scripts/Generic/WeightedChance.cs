using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class WeightedChance<T>
{
    [SerializeField] private UnityDictionary<T, float> options;
    public int Count { get { return options.Count; } }

    //vars
    private float totalChance = 0f;

    //=============== Compile Chances ================
    private void CalcTotalChance()
    {
        totalChance = 0f;
        foreach (float chance in options.Values) {
            totalChance += chance;
        }
    }

    //============== Weigthed Random =================
    public T GetRandom()
    {
        if (totalChance <= 0) { CalcTotalChance(); }

        //choose random option
        float rand = Random.Range(0, totalChance);
        foreach (KeyValuePair<T,float> kvp in options) {
            if (rand < kvp.Value) {
                //found option to pick
                return kvp.Key;
            }
            //continue searching
            else {
                rand -= kvp.Value;
            }
        }
        return default; //should never happen...
    }

    //============= Manage Entries ================
    public void Add(T key, float value)
    {
        if (value <= 0) { value = 1f; } //protect from negative chances
        options.Add(key, value);
        CalcTotalChance();
    }

    public void Remove(T key)
    {
        options.Remove(key);
        CalcTotalChance();
    }

    //============ Contains ===============
    public bool Contains(T key)
    {
        return options.ContainsKey(key);
    }
}