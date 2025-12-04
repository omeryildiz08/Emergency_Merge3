using System.Collections.Generic;
using System;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    // Singleton 
    public static GridManager Instance { get; private set; }

    
    public int gridWidth = 50;
    public int gridHeight = 50;

   
    public GridTileData[,] grid;

    public event Action<MergeableItemData> OnMergeCompleted;
    private void Awake()
    {
        
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

       
        grid = new GridTileData[gridWidth, gridHeight];
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                
                grid[x, y] = new GridTileData();
            }
        }
    }

    
    public void RegisterTile(GridTileView tileView, Vector2Int position)
    {
        if (!IsValidPosition(position)) return; 

        
        grid[position.x, position.y].TileView = tileView;
    }

    
    public void RegisterObject(MergeableObject obj, Vector2Int position)
    {
        if (!IsValidPosition(position)) return;

        
        grid[position.x, position.y].ObjectOnTile = obj;
    }

  
    public void TryMergeOrPlace(MergeableObject movingObject, Vector2Int fromPos, Vector2Int toPos)
    {
       
        if (!IsValidPosition(toPos) || grid[toPos.x, toPos.y].TileView == null)
        {
            SnapObjectToPosition(movingObject, fromPos);
            return;
        }

        MergeableObject targetObj = grid[toPos.x, toPos.y].ObjectOnTile;

        
        if (targetObj != null && targetObj != movingObject)
        {
            
            if (targetObj.ItemData == movingObject.ItemData)
            {
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
        // SENARYO 2: BOŞ BİR KAREYE BIRAKTIK 
        else
        {
            
            ClearCell(fromPos);
            RegisterObject(movingObject, toPos);
            movingObject.CurrentGridPosition = toPos;

            
            List<MergeableObject> mergeGroup = FindMergeGroup(toPos, movingObject.ItemData);

            if (mergeGroup.Count >= 3)
            {
                PerformMerge(mergeGroup, toPos);
            }
            else
            {
                
                SnapObjectToPosition(movingObject, toPos);
            }
        }
    }

    private void PerformMerge(List<MergeableObject> mergeGroup, Vector2Int mergeCenterPos)
    {
        MergeableItemData nextLevelData = mergeGroup[0].ItemData.NextLevelItem;
        if (nextLevelData == null) return;

        
        foreach (var obj in mergeGroup)
        {
            ClearCell(obj.CurrentGridPosition);
            Destroy(obj.gameObject);
        }

        
        if (grid[mergeCenterPos.x, mergeCenterPos.y].TileView != null)
        {
            Vector3 spawnPos = grid[mergeCenterPos.x, mergeCenterPos.y].TileView.GetWorldPosition();
            GameObject newObj = Instantiate(nextLevelData.Prefab, spawnPos, Quaternion.identity);

            MergeableObject newMergeable = newObj.GetComponent<MergeableObject>();
            if (newMergeable != null)
            {
                newMergeable.CurrentGridPosition = mergeCenterPos;
                RegisterObject(newMergeable, mergeCenterPos);

                
                // İleride 
                // buraya "CheckForCombo(newMergeable)" ekleyebiliriz.
            }
            OnMergeCompleted?.Invoke(newMergeable.ItemData);
        }
    }

    private void MoveObject(MergeableObject obj, Vector2Int fromPos, Vector2Int toPos)
    {
        ClearCell(fromPos);
        RegisterObject(obj, toPos); 

       
        obj.CurrentGridPosition = toPos;

        
        SnapObjectToPosition(obj, toPos);
    }

    public void SnapObjectToPosition(MergeableObject obj, Vector2Int pos)
    {
        if (!IsValidPosition(pos) || grid[pos.x, pos.y].TileView == null) return;

      
        obj.transform.position = grid[pos.x, pos.y].TileView.GetWorldPosition();
    }

    private void ClearCell(Vector2Int pos)
    {
        if (IsValidPosition(pos))
        {
            grid[pos.x, pos.y].ObjectOnTile = null;
        }
    }

    private List<MergeableObject> FindMergeGroup(Vector2Int startPos, MergeableItemData targetData)
    {
        List<MergeableObject> group = new List<MergeableObject>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>(); 
        Queue<Vector2Int> toCheck = new Queue<Vector2Int>(); 
        
        toCheck.Enqueue(startPos);
        visited.Add(startPos);

        while (toCheck.Count > 0)
        {
            Vector2Int current = toCheck.Dequeue();
            MergeableObject obj = grid[current.x, current.y].ObjectOnTile;

            
            if (obj != null && obj.ItemData == targetData)
            {
                group.Add(obj);
            }
            
            else if (current != startPos)
            {
                continue;
            }

            // 4 Yöne Bak (Zinciri takip et)
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            foreach (var dir in directions)
            {
                Vector2Int neighborPos = current + dir;

                
                if (IsValidPosition(neighborPos) && !visited.Contains(neighborPos))
                {
                    
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
    private bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridWidth &&
               pos.y >= 0 && pos.y < gridHeight;
    }
    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPosition.x),
            Mathf.RoundToInt(worldPosition.z)
        );
    }
}