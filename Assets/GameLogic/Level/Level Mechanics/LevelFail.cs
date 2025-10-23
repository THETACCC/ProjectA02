using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelFail : MonoBehaviour
{
    public SceneInfo sceneInfo;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }


    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Player1" || collision.gameObject.tag == "Player2" || collision.gameObject.tag == "Player2Tutorial" || collision.gameObject.tag == "Player1Tutorial")
        {
            Scenecontroller.instance.LoadSceneAsset(sceneInfo);
        }
    }

}
