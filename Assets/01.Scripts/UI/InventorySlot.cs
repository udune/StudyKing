using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image icon;
    
    public void Apply(InventoryItem item)
    {
        icon.sprite = Resources.Load<Sprite>($"Texture/{item.name}");
        icon.color = new Color(1, 1, 1, 1);
    }

    public void Clear()
    {
        icon.sprite = null;
        icon.color = new Color(1, 1, 1, 0);
    }
}
