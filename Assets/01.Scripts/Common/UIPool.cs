using System;
using System.Collections.Generic;
using UnityEngine;

public class UIPool : SingletonBehaviour<UIPool>
{
    private Dictionary<Type, Queue<GameObject>> poolDictionary = new Dictionary<Type, Queue<GameObject>>();
    
    public T Get<T>() where T : BaseUI
    {
        var type = typeof(T);
        if (!poolDictionary.ContainsKey(type))
        {
            poolDictionary[type] = new Queue<GameObject>();
        }
        
        var pool = poolDictionary[type];
        if (pool.Count > 0)
        {
            var obj = pool.Dequeue();
            obj.SetActive(true);
            return obj.GetComponent<T>();
        }

        var newObj = Instantiate(Resources.Load<GameObject>($"UI/{type}"));
        return newObj.GetComponent<T>();
    }

    public void Return<T>(T obj) where T : BaseUI
    {
        var type = typeof(T);
        obj.gameObject.SetActive(false);
        poolDictionary[type].Enqueue(obj.gameObject);
    }
}
