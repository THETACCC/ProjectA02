using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubPositionIndicator : MonoBehaviour
{
    private MeshRenderer mr;

    private void Awake()
    {
        mr = GetComponent<MeshRenderer>();  
        mr.enabled = false;
    }
    private void OnMouseEnter()
    {
        mr.enabled = true;
    }

    private void OnMouseExit()
    {
        mr.enabled= false;
    }

    private void OnMouseUpAsButton()
    {
        PlayerCharacter pc = CommonReference.playerCharacters[LevelLoader.PosToMapID(transform.position)];
        pc.transform.position = new Vector3(transform.position.x, pc.transform.position.y, transform.position.z);
    }
}
