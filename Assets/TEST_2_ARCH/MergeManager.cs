using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MergeManager : MonoBehaviour
{
    public static MergeManager Instance;
    public int width = 5;
    public int height = 5;
    public Tile tilePrefab;
    public Transform tileParent;
    public Vector2 origin = Vector2.zero;
    public float tileSpacing = 1.1f;

    private Tile[,] grid;
    public bool isProcessing { get; private set; } = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        CreateGrid();
    }

    void CreateGrid()
    {
        grid = new Tile[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(origin.x + x * tileSpacing, origin.y + y * -tileSpacing, 0);
                Tile t = Instantiate(tilePrefab, pos, Quaternion.identity, tileParent);
                t.x = x; t.y = y;
                grid[x, y] = t;
            }
    }

    public Tile GetNearestTile(Vector3 worldPos)
    {
        // İki aşamalı: convert world pos to nearest tile index
        Tile best = null;
        float bestDist = float.MaxValue;
        foreach (var t in grid)
        {
            float d = Vector2.SqrMagnitude((Vector2)t.transform.position - (Vector2)worldPos);
            if (d < bestDist)
            {
                bestDist = d;
                best = t;
            }
        }
        return best;
    }

    public void HandleDrop(MergeableItem item, Vector3 dropWorldPos)
    {
        if (isProcessing) { SnapBack(item); return; }

        Tile nearest = GetNearestTile(dropWorldPos);
        if (nearest == null) { SnapBack(item); return; }

        // Eğer hedef tile boşsa veya farklı davranış istersen
        if (!nearest.IsEmpty)
        {
            // Swap? şu an basit: drop disallowed -> snap back
            SnapBack(item);
            return;
        }

        // Place to tile
        StartCoroutine(PlaceAndCheckMerge(item, nearest));
    }

    void SnapBack(MergeableItem item)
    {
        if (item.currentTile != null) item.transform.position = item.currentTile.transform.position;
        else Destroy(item.gameObject);
    }

    IEnumerator PlaceAndCheckMerge(MergeableItem item, Tile dest)
    {
        isProcessing = true;
        // remove from old tile
        if (item.currentTile != null) item.currentTile.ClearItem();

        // animate to new tile (simple Lerp)
        Vector3 target = dest.transform.position;
        float t = 0f;
        Vector3 start = item.transform.position;
        while (t < 1f)
        {
            t += Time.deltaTime * 8f;
            item.transform.position = Vector3.Lerp(start, target, t);
            yield return null;
        }

        dest.SetItem(item);
        item.transform.position = target;
        item.transform.SetParent(dest.transform, true);

        // check connected group
        List<Tile> group = FindConnectedSameItems(dest);
        if (group.Count >= 3)
        {
            yield return StartCoroutine(ProcessMergeGroup(group));
            // After merge, allow chain merges:
            yield return new WaitForSeconds(0.1f);
            // Optionally re-check for further merges (iterate over entire grid or only around)
            yield return StartCoroutine(CheckChains());
        }

        isProcessing = false;
    }

    List<Tile> FindConnectedSameItems(Tile start)
    {
        var result = new List<Tile>();
        if (start.IsEmpty) return result;
        ItemData target = start.currentItem.data;

        var visited = new bool[width, height];
        var q = new Queue<Tile>();
        q.Enqueue(start);
        visited[start.x, start.y] = true;

        while (q.Count > 0)
        {
            var t = q.Dequeue();
            result.Add(t);

            // 4-direction neighbors
            Vector2Int[] nbrs = { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };
            foreach (var n in nbrs)
            {
                int nx = t.x + n.x;
                int ny = t.y + n.y;
                if (nx < 0 || ny < 0 || nx >= width || ny >= height) continue;
                if (visited[nx, ny]) continue;
                var nt = grid[nx, ny];
                if (nt.IsEmpty) { visited[nx, ny] = true; continue; }
                if (nt.currentItem.data == target)
                {
                    visited[nx, ny] = true;
                    q.Enqueue(nt);
                }
            }
        }
        return result;
    }

    IEnumerator ProcessMergeGroup(List<Tile> group)
    {
        // Determine merge result (we assume same ItemData, get nextTier from any)
        ItemData baseData = group[0].currentItem.data;
        ItemData resultData = baseData.nextTier;

        // If no next tier, maybe produce a special reward or block merge
        if (resultData == null)
        {
            // just play sparkle and clear 3 items (optional). For now, don't allow merge.
            // Play feedback
            yield break;
        }

        // Choose center tile to spawn result (center of mass)
        Tile center = group[group.Count / 2];
        // animate removal
        foreach (var t in group)
        {
            // simple scale down
            StartCoroutine(ScaleAndDestroy(t.currentItem.gameObject));
            t.ClearItem();
        }
        yield return new WaitForSeconds(0.25f);

        // spawn result prefab
        GameObject go = Instantiate(resultData.prefab, center.transform.position, Quaternion.identity, center.transform);
        MergeableItem mi = go.GetComponent<MergeableItem>();
        if (mi == null) mi = go.AddComponent<MergeableItem>();
        mi.data = resultData;
        center.SetItem(mi);

        // small pop animation
        yield return StartCoroutine(PopAnimation(mi.gameObject));

        yield return null;
    }

    IEnumerator ScaleAndDestroy(GameObject obj)
    {
        float t = 0f;
        Vector3 start = obj.transform.localScale;
        while (t < 1f)
        {
            t += Time.deltaTime * 8f;
            obj.transform.localScale = Vector3.Lerp(start, Vector3.zero, t);
            yield return null;
        }
        Destroy(obj);
    }

    IEnumerator PopAnimation(GameObject obj)
    {
        Vector3 start = Vector3.zero;
        Vector3 end = Vector3.one;
        obj.transform.localScale = start;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 8f;
            obj.transform.localScale = Vector3.Lerp(start, end, t);
            yield return null;
        }
    }

    IEnumerator CheckChains()
    {
        bool mergedAny = false;
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                var t = grid[x, y];
                if (t.IsEmpty) continue;
                var group = FindConnectedSameItems(t);
                if (group.Count >= 3)
                {
                    mergedAny = true;
                    yield return StartCoroutine(ProcessMergeGroup(group));
                    yield return new WaitForSeconds(0.05f);
                }
            }
        if (mergedAny)
        {
            // recursive chain checks
            yield return StartCoroutine(CheckChains());
        }
    }
}
