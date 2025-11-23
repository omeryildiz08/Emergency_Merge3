using UnityEngine;

public class Tile : MonoBehaviour
{
    public int x, y;
    public MergeableItem currentItem;

    public bool IsEmpty => currentItem == null;

    public void SetItem(MergeableItem item)
    {
        currentItem = item;
        if (item != null) item.currentTile = this;
    }

    public void ClearItem()
    {
        currentItem = null;
    }
}
