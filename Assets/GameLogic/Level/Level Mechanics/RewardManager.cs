using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardManager : MonoBehaviour
{

    LevelController levelcontroller;

    private bool isRewardSetup = false;
    // This array will be populated at runtime with all GameObjects tagged "Reward"
    public GameObject[] RewardObjects;

    // Internal cache: one RewardAcquire per RewardObject
    private RewardAcquire[] rewardAcquireScripts;

    // Flags to remember if we've already counted a given RewardAcquire.isReached
    private bool[] rewardReachedFlags;

    // The total number of rewards that have become "isReached == true" so far
    public int rewardsReachedCount = 0;
    //public GameObject[] RewardObjects;
    private LevelController levelController;

    // Start is called before the first frame update
    void Start()
    {
        GameObject levelcontrollerOBVJ = GameObject.FindGameObjectWithTag("LevelPhaseControll");
        levelcontroller = levelcontrollerOBVJ.GetComponent<LevelController>();
    }

    // Update is called once per frame
    void Update()
    {
        // Once the level is in the Running phase, perform the one-time search
        if (levelcontroller != null
            && levelcontroller.phase == LevelPhase.Running
            && !isRewardSetup)
        {
            SreachRewardObjects();
            isRewardSetup = true;
        }

        // After setup is done, continuously check for newly reached rewards
        if (isRewardSetup)
        {
            UpdateRewardCount();
        }
    }

    void SreachRewardObjects()
    {
        // 1. Find every GameObject tagged "Reward"
        RewardObjects = GameObject.FindGameObjectsWithTag("Reward");

        // 2. Allocate arrays of the same length
        int count = RewardObjects.Length;
        rewardAcquireScripts = new RewardAcquire[count];
        rewardReachedFlags = new bool[count];

        // 3. For each reward GameObject, get its RewardAcquire component
        for (int i = 0; i < count; i++)
        {
            GameObject go = RewardObjects[i];
            RewardAcquire acquireScript = go.GetComponent<RewardAcquire>();

            if (acquireScript != null)
            {
                rewardAcquireScripts[i] = acquireScript;
                // Initialize the flag to whatever the current isReached is
                rewardReachedFlags[i] = acquireScript.isReached;
            }
            else
            {
                // If any Reward©\tagged object lacks RewardAcquire, log a warning
                Debug.LogWarning("[RewardManager] GameObject '"
                    + go.name
                    + "' has tag 'Reward' but does not contain a RewardAcquire component.");
                rewardAcquireScripts[i] = null;
                rewardReachedFlags[i] = false;
            }
        }

        Debug.Log("[RewardManager] Found "
            + count
            + " reward object(s) in the scene.");
    }

    void UpdateRewardCount()
    {
        if (rewardAcquireScripts == null)
            return;

        for (int i = 0; i < rewardAcquireScripts.Length; i++)
        {
            RewardAcquire script = rewardAcquireScripts[i];

            if (script == null)
                continue;

            // If this reward has just become "reached" but wasn't before
            if (script.isReached && !rewardReachedFlags[i])
            {
                // 1) Increment the total
                rewardsReachedCount++;

                // 2) Mark as counted
                rewardReachedFlags[i] = true;

                // 3) (Optional) Log or fire off an event
                Debug.Log("[RewardManager] Reward reached on '"
                    + RewardObjects[i].name
                    + "'. Total reached: "
                    + rewardsReachedCount);
            }
        }
    }

    void OnLevelComplete()
    {
        int chapter = levelController.chapterIndex;    // Chapter #: 0 or 1
        int thisLevelRewardCount = rewardsReachedCount; // your running total for this level

        Debug.Log($"Saved {thisLevelRewardCount} rewards to Chapter {chapter}. Total now: " +
                  $"{SaveManager.Instance.GetChapterTotal(chapter)}");
    }

}
