using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BlockCldGenerator))]
public class BlockCldGeneratorEditor : Editor
{
    BlockCldGenerator b;
    private void OnEnable()
    {
        b = (BlockCldGenerator)target;
        if (BlockCldGenerator.blockRef == null)
        {
            BlockCldGenerator.blockRef = GameObject.FindGameObjectWithTag(CommonReference.TAG_BLOCK_REF);
            BoxCollider[] clds = BlockCldGenerator.blockRef.GetComponents<BoxCollider>(); 
            for (int i = 0; i < 9; i++)
            {
                BlockCldGenerator.blockRefSizes[i] = clds[i].size;
                BlockCldGenerator.blockRefCenters[i] = clds[i].center;
            }
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        for(int i = 0; i < 3; i++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int j = 0; j < 3; j++)
            {
                b.cld[i,j]=GUILayout.Toggle(b.cld[i,j],b.cld[i,j]?"1":"0");
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("<Generate>"))
        {
            b.Generate();
        }

        if(GUILayout.Button("<Button 1>"))
        {
            GameObject go = new GameObject("SubBlock");
            go.transform.SetParent(b.transform);
            go.transform.localPosition = Vector3.zero;
        }
    }
}
