using System.Collections.Generic;
using UnityEngine;

public class LevelQuestManager : MonoBehaviour
{
    [Header("Level Ayarları")]
    public List<LevelQuestData> LevelQuests; // Editörden bu levelin görevlerini sürükle
    
    [Header("UI Referansları")]
    public Transform QuestContainer; // UI'da Layout Group olan obje
    public GameObject QuestUIPrefab; // Yukarıdaki scriptin olduğu prefab

   
    private List<QuestUI_Item> activeQuestUIs = new List<QuestUI_Item>();

    void Start()
    {
        // 1. UI'ı oluştur
        foreach (var quest in LevelQuests)
        {
            GameObject uiObj = Instantiate(QuestUIPrefab, QuestContainer);
            QuestUI_Item uiScript = uiObj.GetComponent<QuestUI_Item>();
            uiScript.Setup(quest);
            activeQuestUIs.Add(uiScript);
        }

        // 2. GridManager'ı dinlemeye başla
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