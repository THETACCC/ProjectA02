using SKCell;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FlowManager : SKMonoSingleton<FlowManager>
{
    public static SceneTitle scenetitle;

    private void Start()
    {
        scenetitle = (SceneTitle)PlayerPrefs.GetInt("StartScene");

        SKUtils.InvokeAction(0.2f, () =>
        {
            LoadScene(new SceneInfo()
            {
                index = scenetitle,
            });
        });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SKConsole.Toggle();
        }
    }

    public void LoadScene(SceneInfo info)
    {
        Scenecontroller.instance.LoadSceneAsset(info);

    }

}
