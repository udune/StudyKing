using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image icon;
    
    public void Apply(InventoryItem item)
    {
        icon.sprite = Resources.Load<Sprite>($"Texture/{item.id}");
        icon.color = new Color(1, 1, 1, 1);
        GetComponent<Outline>().enabled = true;
    }

    public void Clear()
    {
        icon.sprite = null;
        icon.color = new Color(1, 1, 1, 0);
        GetComponent<Outline>().enabled = false;
    }
}
