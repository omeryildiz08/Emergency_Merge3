using System.Collections.Generic;
using UnityEngine;

public class LevelQuestManager : MonoBehaviour
{
    [Header("Level Ayarları")]
    public List<LevelQuestData> LevelQuests; 
    
    [Header("UI Referansları")]
    public Transform QuestContainer; 
    public GameObject QuestUIPrefab; 

   
    private List<QuestUI_Item> activeQuestUIs = new List<QuestUI_Item>();

    void Start()
    {
        
        foreach (var quest in LevelQuests)
        {
            GameObject uiObj = Instantiate(QuestUIPrefab, QuestContainer);
            QuestUI_Item uiScript = uiObj.GetComponent<QuestUI_Item>();
            uiScript.Setup(quest);
            activeQuestUIs.Add(uiScript);
        }

        // GridManager'ı dinlemeye başla
        if (GridManager.Instance != null)
        {
            GridManager.Instance.OnMergeCompleted += HandleMergeEvent;
        }
    }

    void OnDestroy()
    {
        
        if (GridManager.Instance != null)
        {
            GridManager.Instance.OnMergeCompleted -= HandleMergeEvent;
        }
    }

    // GridManager "Merge Bitti" dediğinde burası çalışır
    private void HandleMergeEvent(MergeableItemData producedItem)
    {
        // Tüm aktif görevlere bak
        foreach (var questUI in activeQuestUIs)
        {
            LevelQuestData data = questUI.GetQuestData();

            // Eğer görev tipi "Üretim" ise ve üretilen obje doğruysa
            if (data.questType == QuestType.ProduceItem && data.TargetItem == producedItem)
            {
                questUI.AddProgress(1); // İlerlemeyi 1 artır
            }
        }
    }
}