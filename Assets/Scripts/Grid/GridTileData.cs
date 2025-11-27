
public class GridTileData
{
    
    public GridTileView TileView;

    
    public MergeableObject ObjectOnTile;

    // Hızlı bir kontrol: Bu hücrede obje yoksa 'boş' demektir.
    public bool IsEmpty => ObjectOnTile == null;
}