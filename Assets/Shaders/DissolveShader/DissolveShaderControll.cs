using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class DissolveShaderControll : MonoBehaviour
{
    public MeshRenderer myRenderer;
    Material myMaterial;

    private bool keyPressed = false;

    float dissolveOverTime = -2f;


    // Start is called before the first frame update
    void Start()
    {
        myMaterial = myRenderer.material;
        print(myMaterial);
        myMaterial.SetFloat("_Effect_Time", -2f);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.U))
        {
            keyPressed = true;

        }

        if(keyPressed)
        {
            Debug.Log("PRESSED");
            dissolveOverTime += Time.deltaTime;
            myMaterial.SetFloat("_Effect_Time", dissolveOverTime);
        }

    }
}
