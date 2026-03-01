using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSuccessUIManager : MonoBehaviour
{
    public GameObject[] rewardGetOBJ;   // Assign 5 UI objects in Inspector
    public RewardManager rewardManager;

    private int lastCount = -1; // prevents unnecessary updates

    public bool isLevelPass = false;

    void Start()
    {
        GameObject myRewardManager = GameObject.FindGameObjectWithTag("RewardManager");
        if (myRewardManager != null)
        {
            rewardManager = myRewardManager.GetComponent<RewardManager>();
        }

        // Initialize all reward UI objects as disabled
        foreach (GameObject obj in rewardGetOBJ)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }

    void Update()
    {
        if (rewardManager == null) return;
        if (!isLevelPass) return;
        int currentCount = rewardManager.rewardsReachedCount;

        // Only update if count changed
        if (currentCount != lastCount)
        {
            UpdateRewardUI(currentCount);
            lastCount = currentCount;
        }
    }

    private void UpdateRewardUI(int count)
    {
        for (int i = 0; i < rewardGetOBJ.Length; i++)
        {
            if (rewardGetOBJ[i] == null) continue;

            // Enable UI objects for collected rewards
            rewardGetOBJ[i].SetActive(i < count);
        }
    }
}
