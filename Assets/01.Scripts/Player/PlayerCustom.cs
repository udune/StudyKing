using System.Collections.Generic;
using UnityEngine;

public class PlayerCustom : SingletonBehaviour<PlayerCustom>
{
    public Dictionary<string, GameObject> playerItemDict = new Dictionary<string, GameObject>();
    public Dictionary<string, GameObject> characterItemDict = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> cacheDict = new Dictionary<string, GameObject>();
    [SerializeField] Transform player;
    public Transform character;
    
    public void Equip(string name)
    {
        if (playerItemDict.ContainsKey(name) || characterItemDict.ContainsKey(name))
        {
            return;
        }

        if (!cacheDict.ContainsKey(name))
        {
            GameObject obj = Resources.Load<GameObject>($"Item/{name}");
            if (obj == null)
            {
                return;
            }

            cacheDict.Add(name, obj);
        }

        if (CheckItem(player, name, LayerMask.NameToLayer("Player"), out var playerItem))
        {
            playerItem.SetActive(true);
            playerItemDict.Add(name, playerItem);
        }
        
        if (CheckItem(character, name, LayerMask.NameToLayer("Character"), out var characterItem))
        {
            characterItem.SetActive(true);
            characterItemDict.Add(name, characterItem);
        }
    }

    public void UnEquip(string name)
    {
        if (playerItemDict.TryGetValue(name, out var playerItem))
        {
            playerItem.SetActive(false);
            playerItemDict.Remove(name);
        }
        
        if (characterItemDict.TryGetValue(name, out var characterItem))
        {
            characterItem.SetActive(false);
            characterItemDict.Remove(name);
        }
    }

    private void SetLayer(GameObject obj, int layer)
    {
        if (obj == null)
        {
            return;
        }

        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayer(child.gameObject, layer);
        }
    }

    private bool CheckItem(Transform root, string name, int layer, out GameObject result)
    {
        Transform itemParent = root.GetComponent<ItemTrn>().headAndBag;
        Transform item = itemParent.Find(name);

        if (item != null)
        {
            result = item.gameObject;
            return true;
        }

        GameObject newItem = Instantiate(cacheDict[name], itemParent);
        newItem.transform.localPosition = Vector3.zero;
        newItem.name = name;
        SetLayer(newItem, layer);
        result = newItem;
        return true;
    }
}
