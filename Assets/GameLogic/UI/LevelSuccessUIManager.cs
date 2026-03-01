using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSuccessUIManager : MonoBehaviour
{
    public GameObject[] rewardGetOBJ;   // Assign 5 UI objects in Inspector
    public RewardManager rewardManager;

    private int lastCount = -1; // prevents unnecessary updates
    public bool isLevelPass = false;

    // 可选缓存（不用也行）
    private MenuController _menuController;

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

        // 先尝试缓存一次（active 的）
        _menuController = FindObjectOfType<MenuController>();
        if (_menuController == null)
        {
            // 兜底：把 inactive 的也找出来
            var all = Resources.FindObjectsOfTypeAll<MenuController>();
            foreach (var mc in all)
            {
                if (mc == null) continue;
                if (!mc.gameObject.scene.IsValid()) continue;     // 排除 prefab/asset
                if (!mc.gameObject.scene.isLoaded) continue;      // 确保在已加载场景里
                _menuController = mc;
                break;
            }
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
            rewardGetOBJ[i].SetActive(i < count);
        }
    }

    // ✅ 新增：给 Level Success 的 Replay 按钮绑这个
    public void LevelReplay()
    {
        if (_menuController == null)
        {
            _menuController = FindObjectOfType<MenuController>();

            if (_menuController == null)
            {
                var all = Resources.FindObjectsOfTypeAll<MenuController>();
                foreach (var mc in all)
                {
                    if (mc == null) continue;
                    if (!mc.gameObject.scene.IsValid()) continue;
                    if (!mc.gameObject.scene.isLoaded) continue;
                    _menuController = mc;
                    break;
                }
            }
        }

        if (_menuController != null)
        {
            _menuController.ResetCurrentScene();
        }
        else
        {
            Debug.LogError("[LevelSuccessUIManager] MenuController not found in the current scene.");
        }
    }
}