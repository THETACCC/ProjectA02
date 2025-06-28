using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardUIManager : MonoBehaviour
{
    [Header("Reference to the RewardManager that tracks collected rewards")]
    [Tooltip("If not assigned in the Inspector, this script will attempt to find a RewardManager in the scene at runtime.")]
    public RewardManager rewardManager;

    [Header("List of UI image GameObjects to toggle on as rewards are collected")]
    [Tooltip("Order these in the Inspector from first reward ¡ú last reward.")]
    public GameObject[] rewardImageObjects;
    public MMFeedbacks[] RewardFeedbacks;

    // Cache the last known count so we only update the UI when it changes
    private int lastKnownCount = -1;

    void Start()
    {
        // If no RewardManager was assigned in the Inspector, try to find one automatically
        if (rewardManager == null)
        {
            rewardManager = FindObjectOfType<RewardManager>();
            if (rewardManager == null)
            {
                Debug.LogError(
                    "[RewardUIManager] Unable to find a RewardManager in the scene. " +
                    "Please assign one in the Inspector or ensure a RewardManager exists."
                );
            }
        }

        // Optional: initialize all images as inactive
        if (rewardImageObjects != null)
        {
            for (int i = 0; i < rewardImageObjects.Length; i++)
            {
                rewardImageObjects[i].SetActive(false);
            }
        }
    }

    void Update()
    {
        if (rewardManager == null || rewardImageObjects == null)
            return;

        int currentCount = rewardManager.rewardsReachedCount;

        // Only update the UI if the count has changed
        if (currentCount != lastKnownCount)
        {
            lastKnownCount = currentCount;
            RefreshRewardImages(currentCount);
        }
    }

    /// <summary>
    /// Enables the first 'count' images in rewardImageObjects[], disables the others.
    /// </summary>
    /// <param name="count">Number of rewards collected (and thus how many images should be active).</param>
    private void RefreshRewardImages(int count)
    {
        int totalImages = rewardImageObjects.Length;

        for (int i = 0; i < totalImages; i++)
        {
            // If index is less than the number of collected rewards, set active; otherwise, deactivate
            bool shouldBeActive = (i < count);
            if (rewardImageObjects[i].activeSelf != shouldBeActive)
            {
                rewardImageObjects[i].SetActive(shouldBeActive);
                RewardFeedbacks[i]?.PlayFeedbacks();
            }
        }
    }
}
