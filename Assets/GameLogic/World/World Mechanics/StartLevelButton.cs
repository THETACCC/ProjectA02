using UnityEngine;

public class StartLevelButton : MonoBehaviour
{
    public void StartTheLevel()
    {
        // Look for all LevelTrigger components in the parent's children,
        // then choose the one that is closest (by position) to this button.
        LevelTrigger[] triggers = transform.parent.GetComponentsInChildren<LevelTrigger>();

        if (triggers == null || triggers.Length == 0)
        {
            Debug.LogError("No LevelTrigger found among parent's children!");
            return;
        }

        LevelTrigger bestTrigger = null;
        float bestDistance = float.MaxValue;

        foreach (LevelTrigger lt in triggers)
        {
            // Calculate the distance between this button and the LevelTrigger's position.
            float distance = Vector3.Distance(transform.position, lt.transform.position);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestTrigger = lt;
            }
        }

        if (bestTrigger != null)
        {
            Debug.Log("StartLevelButton found LevelTrigger: " + bestTrigger.gameObject.name
                      + " (distance: " + bestDistance + ")");
            bestTrigger.LoadNextLevel();
        }
        else
        {
            Debug.LogError("No suitable LevelTrigger found!");
        }
    }
}
