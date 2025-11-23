using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Merge/ItemData")]
public class ItemData : ScriptableObject
{
    public string id;                // benzersiz ID (ör: "star_seed")
    public int tier;                 // seviye/tier (1,2,3...)
    public GameObject prefab;        // sahneye instantiate edilecek prefab
    public ItemData nextTier;        // 3'ü birleştiğinde hangi ItemData oluşur (null = max)
    public Sprite icon;              // UI için (isteğe bağlı)
}
