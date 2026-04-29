using UnityEngine;

public class TutorialNotebookOpener : MonoBehaviour
{
    [Header("Menu Controller")]
    [SerializeField] private MenuController menuController;

    [Header("Notebook Objects")]
    [SerializeField] private GameObject notebookPageRoot;
    [SerializeField] private GameObject specificPage;

    [Header("Optional")]
    [SerializeField] private bool closeOtherNotebookPagesFirst = true;
    [SerializeField] private bool openOnStart = false;

    private void Start()
    {
        if (openOnStart)
        {
            OpenTutorialNotebook();
        }
    }

    public void OpenTutorialNotebook()
    {
        if (menuController == null)
        {
            menuController = FindObjectOfType<MenuController>();
        }

        if (menuController != null)
        {
            menuController.OpenBouncyObject();

            if (closeOtherNotebookPagesFirst)
            {
                menuController.NotebookAllDeactivate();
            }
        }

        if (notebookPageRoot != null)
        {
            notebookPageRoot.SetActive(true);
        }

        if (specificPage != null)
        {
            specificPage.SetActive(true);
        }
    }
}