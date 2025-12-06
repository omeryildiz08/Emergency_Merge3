using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum QuestType 
    {
        ProduceItem,
        MergeCountLimit

    }

[CreateAssetMenu(fileName ="New Quest", menuName ="Merge Game/Level Quest")]
public class LevelQuestData : ScriptableObject
{
    [Header("Görsel Ayarlar")]
    public string Description; //1 adet x objesi oluştur
    public Sprite Icon; //sol tarafta duracak tıklanınca questi açacak icon

    [Header("Görev Mantığı")]
    public QuestType questType;

    public MergeableItemData TargetItem;

    
    public int RequiredAmount = 1;
    
}
