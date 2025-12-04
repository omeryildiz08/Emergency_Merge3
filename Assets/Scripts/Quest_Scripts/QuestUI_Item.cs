using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestUI_Item : MonoBehaviour
{
    [Header("UI Bağlantıları")]
    public Image IconImage;           // Soldaki Yıldız
    public GameObject DescriptionPanel; // Açılan balon (Card)
    public TextMeshProUGUI DescriptionText;
    public TextMeshProUGUI ProgressText; // "1/5" gibi
    public GameObject Checkmark;      // Görev bitince çıkan tik işareti
    private LevelQuestData myQuestData;
    private int currentAmount;
    private bool isCompleted = false;

    public void Setup(LevelQuestData data)
    {
        myQuestData = data;
        IconImage.sprite = data.Icon;
        DescriptionText.text = data.Description;
        currentAmount = 0;

        UpdateProgressText();
        DescriptionPanel.SetActive(false); // Başta kapalı olsun
        Checkmark.SetActive(false);
    }

    public void ToggleDescription()
    {
        bool isActive = DescriptionPanel.activeSelf;
        DescriptionPanel.SetActive(!isActive);
    }

    public void AddProgress(int amount)
    {
        if (isCompleted) return;

        currentAmount += amount;
        UpdateProgressText();

        if (currentAmount >= myQuestData.RequiredAmount)
        {
            CompleteQuest();
        }
    }

    void UpdateProgressText()
    {
        ProgressText.text = $"{currentAmount}/{myQuestData.RequiredAmount}";
    }

    void CompleteQuest()
    {
        isCompleted = true;
        Checkmark.SetActive(true);
        ProgressText.text = "TAMAMLANDI";
        // Burada bir ses veya efekt çalabilirsin
    }

    public LevelQuestData GetQuestData() { return myQuestData; }
}
