using System.Collections.Generic;
using UnityEngine;

public class PlayerCustom : SingletonBehaviour<PlayerCustom>
{
    public Dictionary<string, GameObject> playerItemDict = new Dictionary<string, GameObject>();
    public Dictionary<string, GameObject> characterItemDict = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> cacheDict = new Dictionary<string, GameObject>();
    [SerializeField] Transform player;
    public Transform character;
    
    public void Equip(string id)
    {
        if (playerItemDict.ContainsKey(id) || characterItemDict.ContainsKey(id))
        {
            return;
        }

        if (!cacheDict.ContainsKey(id))
        {
            GameObject obj = Resources.Load<GameObject>($"Item/{id}");
            if (obj == null)
            {
                return;
            }

            cacheDict.Add(id, obj);
        }

        if (CheckItem(player, id, LayerMask.NameToLayer("Player"), out var playerItem))
        {
            playerItem.SetActive(true);
            playerItemDict.Add(id, playerItem);
        }
        
        if (CheckItem(character, id, LayerMask.NameToLayer("Character"), out var characterItem))
        {
            characterItem.SetActive(true);
            characterItemDict.Add(id, characterItem);
        }
    }

    public void UnEquip(string id)
    {
        if (playerItemDict.TryGetValue(id, out var playerItem))
        {
            playerItem.SetActive(false);
            playerItemDict.Remove(id);
        }
        
        if (characterItemDict.TryGetValue(id, out var characterItem))
        {
            characterItem.SetActive(false);
            characterItemDict.Remove(id);
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

    private bool CheckItem(Transform root, string id, int layer, out GameObject result)
    {
        Transform itemParent = root.GetComponent<ItemTrn>().headAndBag;
        Transform item = itemParent.Find(id);

        if (item != null)
        {
            result = item.gameObject;
            return true;
        }

        GameObject newItem = Instantiate(cacheDict[id], itemParent);
        newItem.transform.localPosition = Vector3.zero;
        newItem.name = id;
        SetLayer(newItem, layer);
        result = newItem;
        return true;
    }
}
