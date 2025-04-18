using UnityEngine;
using TMPro;

public class GetLevelName : MonoBehaviour
{
    private TMP_Text tmpText;

    void Start()
    {
        tmpText = GetComponent<TMP_Text>();
        if (tmpText == null)
        {
            //Debug.LogError("No TMP_Text component found on this GameObject!");
            return;
        }

        Transform parentTransform = transform.parent;

        // Go up three levels (parent's parent's parent)
        for (int i = 1; i <= 2; i++)
        {
            if (parentTransform != null)
            {
                //Debug.Log($"Level {i} parent name: {parentTransform.gameObject.name}");
                parentTransform = parentTransform.parent;
            }
            else
            {
                Debug.LogError($"No parent at level {i}.");
                return;
            }
        }

        if (parentTransform != null)
        {
            tmpText.text = parentTransform.gameObject.name;
            //print(parentTransform.gameObject.name);
        }
        else
        {
            Debug.LogError("The specified parent level (3 levels up) doesn't exist.");
        }
    }
}
