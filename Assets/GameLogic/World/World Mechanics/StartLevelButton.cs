using UnityEngine;

public class StartLevelButton : MonoBehaviour
{
    private LevelTrigger levelTrigger;


    /// <summary>
    /// Call this (for example via a UI Button OnClick) to load the next level.
    /// </summary>
    public void StartTheLevel()
    {
        // Look for a LevelTrigger component up the parent chain
        Transform currentParent = transform.parent;
        while (currentParent != null)
        {
            levelTrigger = currentParent.GetComponent<LevelTrigger>();
            if (levelTrigger != null)
                break;

            currentParent = currentParent.parent;
        }


        if (levelTrigger != null)
        {
            levelTrigger.LoadNextLevel();
        }
        else
        {
            Debug.LogError($"[{name}] LevelTrigger reference is missing!", this);
        }
    }
}
