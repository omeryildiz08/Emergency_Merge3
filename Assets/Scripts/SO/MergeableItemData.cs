using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New MergeableItem", menuName = "Merge Game/Mergeable Item")]
public class MergeableItemData : ScriptableObject
{
    
    public string ItemName;

    
    public int Level;

   
    public GameObject Prefab;

   
    public MergeableItemData NextLevelItem;
}
