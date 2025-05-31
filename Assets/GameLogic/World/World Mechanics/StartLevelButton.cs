using UnityEngine;
using UnityEngine.UI;  // ← required for "Button"

public class StartLevelButton : MonoBehaviour
{
    private LevelTrigger levelTrigger;

    void Awake()
    {
        // Remove ANY old listeners that might have been baked into a prefab.
        // This prevents the button from still pointing at Level 5’s method.
        Button uiBtn = GetComponent<Button>();
        if (uiBtn != null)
            uiBtn.onClick.RemoveAllListeners();
    }

    void Start()
    {
        // 1) Auto-find the closest LevelTrigger in our parent chain.
        //    This climbs up from Btn_Start → ... → LevelX, and picks up LevelTrigger there.
        levelTrigger = GetComponentInParent<LevelTrigger>();

        if (levelTrigger == null)
        {
            Debug.LogError($"[StartLevelButton:{name}] ❌ Could not find a LevelTrigger in any parent! " +
                           $"Make sure this button is nested under the GameObject that has LevelTrigger attached.", this);
            return;
        }

        // 2) Log exactly which LevelTrigger we found and what SceneTitle it uses.
        Debug.Log($"[StartLevelButton:{name}] ✅ Found parent LevelTrigger: '{levelTrigger.name}' (scenetitle = {levelTrigger.scenetitle})", levelTrigger);

        // 3) Now hook up our own StartTheLevel() to the Button’s onClick.
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(StartTheLevel);
        }
        else
        {
            Debug.LogWarning($"[StartLevelButton:{name}] ⚠️ No Button component found! This script must be on a UI Button GameObject.", this);
        }
    }

    public void StartTheLevel()
    {
        // Called when the player clicks the “Start” UI button.
        if (levelTrigger == null)
        {
            Debug.LogError($"[StartLevelButton:{name}] ❌ levelTrigger is null at click time!", this);
            return;
        }

        // Log so you can watch in the Console which trigger is actually running:
        Debug.Log($"[StartLevelButton:{name}] ▶ CLICK → Calling LoadNextLevel() on '{levelTrigger.name}' (scenetitle = {levelTrigger.scenetitle})", levelTrigger);

        // Finally, load the next level on that trigger:
        levelTrigger.LoadNextLevel();
    }
}
