using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;  // Required for Button
using TMPro;

public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Button button; // Reference to the Button component
    private TextMeshProUGUI textMeshPro; // Reference to the TextMeshPro component

    private void Awake()
    {
        // Automatically get the Button component attached to the same GameObject
        button = GetComponent<Button>();
        // Automatically get the TextMeshPro component
        textMeshPro = GetComponentInChildren<TextMeshProUGUI>();

        // Optional: You can also add a listener to the button's onClick event
        button.onClick.AddListener(OnButtonClick);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Change color on hover
        if (textMeshPro != null)
        {
            textMeshPro.color = Color.yellow; // Change to desired hover color
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Reset color when not hovering
        if (textMeshPro != null)
        {
            textMeshPro.color = Color.white; // Change back to original color
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // This will also be called when the button is clicked
        OnButtonClick();
    }

    private void OnButtonClick()
    {
        // Handle button click
        Debug.Log("Button clicked: " + gameObject.name);
        // Add your button click logic here
    }
}
