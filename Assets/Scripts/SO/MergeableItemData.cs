using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Bu satır, Unity editöründe "Create" menüsüne bir seçenek ekler.
[CreateAssetMenu(fileName = "New MergeableItem", menuName = "Merge Game/Mergeable Item")]
public class MergeableItemData : ScriptableObject
{
    // Objenin adı (debug için)
    public string ItemName;

    // Objenin seviyesi (örn: Tomurcuk=1, Çiçek=2)
    public int Level;

    // Bu objenin 3D modeli.
    // Demo için senin hazırladığın (örn: Kırmızı Küre) prefab'ını buraya sürükleyeceksin.
    public GameObject Prefab;

    // Birleşme (merge) gerçekleştiğinde hangi objeye dönüşeceği.
    // Örn: Tomurcuk_L1'in bu alanına Cicek_L2'yi sürükleyeceksin.
    // En son seviyede (örn: BuyukCicek_L3) burası 'null' (None) kalmalı.
    public MergeableItemData NextLevelItem;
}
