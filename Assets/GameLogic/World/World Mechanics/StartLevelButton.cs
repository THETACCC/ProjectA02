using UnityEngine;

public class StartLevelButton : MonoBehaviour
{
    private LevelTrigger levelTrigger;

    void Start()
    {
        Transform currentParent = transform.parent;

        // Traverse upwards through parents until LevelTrigger is found or root is reached
        while (currentParent != null)
        {
            levelTrigger = currentParent.GetComponent<LevelTrigger>();
            if (levelTrigger != null)
                break;

            currentParent = currentParent.parent;
        }

        if (levelTrigger == null)
        {
            Debug.LogError("NextLevelButton could not find LevelTrigger in parent hierarchy!");
        }
    }

    public void StartTheLevel()
    {
        if (levelTrigger != null)
        {
            levelTrigger.LoadNextLevel();
        }
        else
        {
            Debug.LogError("LevelTrigger reference missing!");
        }
    }
}
