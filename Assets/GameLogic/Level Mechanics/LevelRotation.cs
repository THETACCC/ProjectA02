using SKCell;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelRotation : MonoBehaviour
{

    private Transform self;


    private void Start()
    {
        self = gameObject.transform; 
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            Vector3 orot = self.rotation.eulerAngles;

            RotateTo(self.rotation.eulerAngles + new Vector3(0, 90, 0));

        }
    }


    private void RotateTo(Vector3 rot)
    {
        Vector3 orot = self.rotation.eulerAngles;
        CommonUtils.StartProcedure(SKCurve.QuadraticDoubleIn, 1f, (f) =>
        {
            self.rotation = Quaternion.Euler(Vector3.Lerp(orot, rot, f));
        });
    }
}
