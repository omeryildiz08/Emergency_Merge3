[System.Serializable]
public class GridTileData
{
    
    [System.NonSerialized]
    public GridTileView TileView;

    [System.NonSerialized]
    public MergeableObject ObjectOnTile;

    // Hızlı bir kontrol: Bu hücrede obje yoksa 'boş' demektir.
    public bool IsEmpty => ObjectOnTile == null;

    //gdd v.04 
    public bool isLocked = false;
    public bool hasAnomaly = false;
}