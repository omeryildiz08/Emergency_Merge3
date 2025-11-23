using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    // Singleton Pattern
    public static GridManager Instance { get; private set; }

    // Haritanın maksimum boyutları
    public int gridWidth = 50;
    public int gridHeight = 50;

    // BEYİN: Tüm mantıksal grid verisini tutan 2D dizi.
    public GridTileData[,] grid;

    private void Awake()
    {
        // Singleton kurulumu
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        // Mantıksal grid'i ilklendir (initialize et)
        grid = new GridTileData[gridWidth, gridHeight];
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                // Her hücreyi boş bir data objesiyle doldur
                grid[x, y] = new GridTileData();
            }
        }
    }

    // --- Kayıt Fonksiyonları (Oyun Başlarken Çağrılır) ---

    // GridTileView.cs, Start() fonksiyonunda burayı çağırır.
    public void RegisterTile(GridTileView tileView, Vector2Int position)
    {
        if (!IsValidPosition(position)) return; // Sınır kontrolü

        // Mantıksal grid'e "Bu koordinatın görseli budur" diyoruz.
        grid[position.x, position.y].TileView = tileView;
    }

    // MergeableObject.cs, Start() fonksiyonunda burayı çağırır.
    public void RegisterObject(MergeableObject obj, Vector2Int position)
    {
        if (!IsValidPosition(position)) return;

        // Mantıksal grid'e "Bu koordinatın üzerinde bu obje var" diyoruz.
        grid[position.x, position.y].ObjectOnTile = obj;
    }

    // --- Çekirdek Merge/Move Mantığı ---

    // MergeableObject.cs, OnMouseUp() fonksiyonunda burayı çağırır.
    public void TryMergeOrPlace(MergeableObject movingObject, Vector2Int fromPos, Vector2Int toPos)
    {
        // 1. Gidilen yer geçerli bir tile mı? (Elle delikli harita yaptıysak?)
        if (!IsValidPosition(toPos) || grid[toPos.x, toPos.y].TileView == null)
        {
            // Geçerli değilse, objeyi eski yerine geri "snap" et (oturt).
            SnapObjectToPosition(movingObject, fromPos);
            return;
        }

        // 2. Birleşme (Merge) kontrolü
        List<MergeableObject> neighborsToMerge = FindNeighborsToMerge(toPos, movingObject.ItemData);

        // Yeterli komşu bulundu mu? (En az 2 komşu + 1 bırakılan = 3'lü merge)
        if (neighborsToMerge.Count >= 2)
        {
            PerformMerge(movingObject, neighborsToMerge, toPos);
        }
        // 3. Taşıma (Move) kontrolü
        else if (grid[toPos.x, toPos.y].IsEmpty)
        {
            // Bırakılan yer boşsa, objeyi oraya taşı
            MoveObject(movingObject, fromPos, toPos);
        }
        // 4. Başarısız (Yer dolu ve merge yok)
        else
        {
            // Gidilen yer doluysa, objeyi eski yerine geri "snap" et.
            SnapObjectToPosition(movingObject, fromPos);
        }
    }

    private void PerformMerge(MergeableObject droppedObject, List<MergeableObject> neighbors, Vector2Int mergePos)
    {
        // 1. Yeni objenin verisini al
        MergeableItemData nextLevelData = droppedObject.ItemData.NextLevelItem;

        // En yüksek seviyedeki bir objeyi birleştirmeye çalışıyorsak (NextLevelItem = null)
        // merge işlemini iptal et ve objeyi eski yerine yolla (veya en yakın boşluğa)
        if (nextLevelData == null)
        {
            SnapObjectToPosition(droppedObject, droppedObject.CurrentGridPosition);
            return;
        }

        // 2. Eski objeleri yok et
        // Önce bırakılanı yok et
        ClearCell(droppedObject.CurrentGridPosition);
        Destroy(droppedObject.gameObject);

        // Sonra komşuları yok et
        foreach (var neighbor in neighbors)
        {
            ClearCell(neighbor.CurrentGridPosition);
            Destroy(neighbor.gameObject);
        }

        // 3. Yeni objeyi yarat (Instantiate)
        GameObject newObjectInstance = Instantiate(
            nextLevelData.Prefab,
            grid[mergePos.x, mergePos.y].TileView.GetWorldPosition(), // Tile'ın merkezine koy
            Quaternion.identity
        );

        // 4. Yeni objeyi grid'e kaydet
        MergeableObject newMergeable = newObjectInstance.GetComponent<MergeableObject>();
        newMergeable.CurrentGridPosition = mergePos; // Pozisyonunu set et
        RegisterObject(newMergeable, mergePos); // Grid'e kaydet
    }

    private void MoveObject(MergeableObject obj, Vector2Int fromPos, Vector2Int toPos)
    {
        // Mantıksal grid'i güncelle
        ClearCell(fromPos); // Eski yeri boşalt
        RegisterObject(obj, toPos); // Yeni yere kaydet

        // Objenin kendi pozisyon bilgisini güncelle
        obj.CurrentGridPosition = toPos;

        // Görsel olarak objeyi yeni tile'ın merkezine oturt
        SnapObjectToPosition(obj, toPos);
    }

    // --- Yardımcı Fonksiyonlar ---

    // Bir objeyi görsel olarak bir tile'ın merkezine "oturtur".
    public void SnapObjectToPosition(MergeableObject obj, Vector2Int pos)
    {
        if (!IsValidPosition(pos) || grid[pos.x, pos.y].TileView == null) return;

        // Tile'ın dünya pozisyonunu al ve objeyi oraya koy
        obj.transform.position = grid[pos.x, pos.y].TileView.GetWorldPosition();
    }

    // Bir grid hücresindeki objeyi MANTIKSAL olarak temizler.
    private void ClearCell(Vector2Int pos)
    {
        if (IsValidPosition(pos))
        {
            grid[pos.x, pos.y].ObjectOnTile = null;
        }
    }

    // Verilen pozisyonun 4 komşusunu (K, G, D, B) kontrol eder
    // ve merge için uygun olanları (aynı ItemData'ya sahip) döndürür.
    private List<MergeableObject> FindNeighborsToMerge(Vector2Int pos, MergeableItemData itemData)
    {
        List<MergeableObject> neighbors = new List<MergeableObject>();

        Vector2Int[] directions = {
            Vector2Int.up,    // (0, 1)
            Vector2Int.down,  // (0, -1)
            Vector2Int.left,  // (-1, 0)
            Vector2Int.right  // (1, 0)
        };

        foreach (var dir in directions)
        {
            Vector2Int neighborPos = pos + dir;
            if (IsValidPosition(neighborPos))
            {
                MergeableObject neighborObj = grid[neighborPos.x, neighborPos.y].ObjectOnTile;
                // Komşu boş değilse VE ItemData'sı bizimkiyle aynıysa
                if (neighborObj != null && neighborObj.ItemData == itemData)
                {
                    neighbors.Add(neighborObj);
                }
            }
        }
        return neighbors;
    }

    // Bir pozisyonun grid sınırları içinde olup olmadığını kontrol eder.
    private bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridWidth &&
               pos.y >= 0 && pos.y < gridHeight;
    }

    // Dünya pozisyonunu (örn: mouse'un tıkladığı yer) en yakın
    // grid koordinatına (örn: [2, 3]) çevirir.
    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPosition.x),
            Mathf.RoundToInt(worldPosition.z)
        );
    }
}