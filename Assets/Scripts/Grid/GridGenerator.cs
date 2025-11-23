using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    // Artistlerin yaptığı prefab'ları buraya sürükleyeceksin
    public GameObject grassTilePrefab;
    //public GameObject deadTilePrefab; // Henüz kullanmasak da dursun

    public int gridWidth = 10;
    public int gridHeight = 10;

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        // Döngü ile 10x10'luk bir alan yarat
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                // Her bir kare için pozisyon belirle
                // (x, 0, z) -> 1'er birim aralıkla dizer
                Vector3 tilePosition = new Vector3(x, 0, z);

                // Prefab'ı o pozisyonda yarat (Instantiate)
                Instantiate(grassTilePrefab, tilePosition, Quaternion.identity, this.transform);
                
                // NOT: Gerçek bir oyunda hangi karonun 'dead'
                // hangisinin 'grass' olacağına bir level datasına
                // göre karar veririz, ama demo için hepsi grass olabilir.
            }
        }
    }
}
