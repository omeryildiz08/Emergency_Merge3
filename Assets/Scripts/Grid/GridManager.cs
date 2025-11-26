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
        // 1. Gidilen yer geçerli mi?
        if (!IsValidPosition(toPos) || grid[toPos.x, toPos.y].TileView == null)
        {
            SnapObjectToPosition(movingObject, fromPos);
            return;
        }

        MergeableObject targetObj = grid[toPos.x, toPos.y].ObjectOnTile;

        // SENARYO 1: BAŞKA BİR OBJENİN ÜZERİNE BIRAKTIK (Merge Tetikleme)
        if (targetObj != null && targetObj != movingObject)
        {
            // Aynı tipteyse zinciri kontrol et
            if (targetObj.ItemData == movingObject.ItemData)
            {
                // Hedef noktayı merkez alarak bağlı tüm grubu bul
                // (movingObject henüz grid'de olmadığı için listeye manuel ekleyeceğiz)
                List<MergeableObject> mergeGroup = FindMergeGroup(toPos, movingObject.ItemData);

                if (!mergeGroup.Contains(movingObject)) mergeGroup.Add(movingObject);

                if (mergeGroup.Count >= 3)
                {
                    PerformMerge(mergeGroup, toPos);
                    return;
                }
            }
            // Tip farklıysa veya sayı yetmediyse geri dön
            SnapObjectToPosition(movingObject, fromPos);
        }
        // SENARYO 2: BOŞ BİR KAREYE BIRAKTIK (Taşıma ve Zincir Kontrolü)
        else
        {
            // Önce mantıksal olarak taşı
            ClearCell(fromPos);
            RegisterObject(movingObject, toPos);
            movingObject.CurrentGridPosition = toPos;

            // Şimdi yeni yerinden etrafına bak, zincir var mı?
            List<MergeableObject> mergeGroup = FindMergeGroup(toPos, movingObject.ItemData);

            if (mergeGroup.Count >= 3)
            {
                PerformMerge(mergeGroup, toPos);
            }
            else
            {
                // Merge yok, sadece yerine oturt
                SnapObjectToPosition(movingObject, toPos);
            }
        }
    }

    private void PerformMerge(List<MergeableObject> mergeGroup, Vector2Int mergeCenterPos)
    {
        MergeableItemData nextLevelData = mergeGroup[0].ItemData.NextLevelItem;
        if (nextLevelData == null) return;

        // Gruptaki TÜM objeleri yok et (Zincirdeki herkes gider)
        foreach (var obj in mergeGroup)
        {
            ClearCell(obj.CurrentGridPosition);
            Destroy(obj.gameObject);
        }

        // Yeni objeyi yarat
        // Tile'ın pozisyonunu güvenli şekilde al
        if (grid[mergeCenterPos.x, mergeCenterPos.y].TileView != null)
        {
            Vector3 spawnPos = grid[mergeCenterPos.x, mergeCenterPos.y].TileView.GetWorldPosition();
            GameObject newObj = Instantiate(nextLevelData.Prefab, spawnPos, Quaternion.identity);

            MergeableObject newMergeable = newObj.GetComponent<MergeableObject>();
            if (newMergeable != null)
            {
                newMergeable.CurrentGridPosition = mergeCenterPos;
                RegisterObject(newMergeable, mergeCenterPos);

                // YENİ EKLENTİ: Yaratılan objenin zincirleme reaksiyon yaratma ihtimali (Combo)
                // İleride buraya "CheckForCombo(newMergeable)" ekleyebiliriz.
            }
        }
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

    // YENİ FONKSİYON: Sadece yanındakine değil, tüm zincire bakar (Recursive/Queue)
    private List<MergeableObject> FindMergeGroup(Vector2Int startPos, MergeableItemData targetData)
    {
        List<MergeableObject> group = new List<MergeableObject>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>(); // Ziyaret edilenleri not et (Sonsuz döngü olmasın)
        Queue<Vector2Int> toCheck = new Queue<Vector2Int>(); // Kontrol edilecekler kuyruğu

        // Aramaya başlanacak noktayı ekle
        toCheck.Enqueue(startPos);
        visited.Add(startPos);

        while (toCheck.Count > 0)
        {
            Vector2Int current = toCheck.Dequeue();
            MergeableObject obj = grid[current.x, current.y].ObjectOnTile;

            // Eğer bu karedeki obje aradığımız tipteyse gruba al
            // (Başlangıç noktası boş olsa bile komşularına bakmak için devam etmeliyiz)
            if (obj != null && obj.ItemData == targetData)
            {
                group.Add(obj);
            }
            // Eğer burası boşsa veya farklı tipteyse ve burası BAŞLANGIÇ NOKTASI DEĞİLSE, zincir kopmuştur.
            else if (current != startPos)
            {
                continue;
            }

            // 4 Yöne Bak (Zinciri takip et)
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            foreach (var dir in directions)
            {
                Vector2Int neighborPos = current + dir;

                // Geçerli mi ve daha önce bakmadık mı?
                if (IsValidPosition(neighborPos) && !visited.Contains(neighborPos))
                {
                    // Sadece DOLU ve AYNI TİP olan komşuları kuyruğa ekle ki arama orada devam etsin
                    MergeableObject neighborObj = grid[neighborPos.x, neighborPos.y].ObjectOnTile;
                    if (neighborObj != null && neighborObj.ItemData == targetData)
                    {
                        visited.Add(neighborPos);
                        toCheck.Enqueue(neighborPos);
                    }
                }
            }
        }
        return group;
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