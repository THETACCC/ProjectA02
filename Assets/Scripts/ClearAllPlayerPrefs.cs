using UnityEngine;

public class ClearAllPlayerPrefs : MonoBehaviour
{
    //TESTING PURPOSE ONLY!!!


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            // Delete only the specific keys you want to reset
            PlayerPrefs.DeleteKey("LastTriggerX");
            PlayerPrefs.DeleteKey("LastTriggerY");
            PlayerPrefs.DeleteKey("LastTriggerZ");
            PlayerPrefs.DeleteKey("LastTriggerScene");

            Debug.Log("Specific PlayerPrefs data cleared, but player's saved progress remains intact!");
        }
    }

}
