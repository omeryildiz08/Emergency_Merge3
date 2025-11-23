// Bu class'ın MonoBehaviour'dan miras almasına GEREK YOK.
// Bu sadece bir veri tutucudur (data container).
public class GridTileData
{
    // O grid hücresinin sahnede elle yerleştirdiğin
    // görsel karşılığı (temp_Tile objesi)
    public GridTileView TileView;

    // O grid hücresinin ÜZERİNDE duran birleşebilir obje (Tomurcuk vb.)
    public MergeableObject ObjectOnTile;

    // Hızlı bir kontrol: Bu hücrede obje yoksa 'boş' demektir.
    public bool IsEmpty => ObjectOnTile == null;
}