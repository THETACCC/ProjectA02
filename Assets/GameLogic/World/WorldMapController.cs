using TMPro;
using UnityEngine;

public class WorldMapController : MonoBehaviour
{
    public int chapterIndex = 0;
    public TextMeshProUGUI totalRewardsText;   

    void Start()
    {
        UpdateUI();
    }

    // also run when re-enabling the map
    void OnEnable() => UpdateUI();

    void UpdateUI()
    {
        int total = SaveManager.Instance.GetChapterTotal(chapterIndex);
        totalRewardsText.text = $"Rewards: {total}";
    }
}
