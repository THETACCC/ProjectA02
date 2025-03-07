using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationPivotCalculate : MonoBehaviour
{

    public float PivotPosX;
    public float PivotPosY;
    public GameObject PivotOBJ;
    public GameObject ResetOBJ;
    // Start is called before the first frame update
    void Start()
    {

        ResetOBJ.transform.position = ResetOBJ.transform.position - new Vector3(PivotPosX, 0, PivotPosY);
        PivotOBJ.transform.position = this.gameObject.transform.position + new Vector3(PivotPosX, 0 , PivotPosY);

        //this.gameObject.transform.position = this.gameObject.transform.position + new Vector3(PivotPosX, 0, PivotPosY);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
