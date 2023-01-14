using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestControls : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U)) 
        {
            ScreenShakeFromAnimationCurve2D.Instance.ShakeCameraFromCurve();
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            ScreenShakeFromAnimationCurve2D.Instance.ShakeCameraFromCurve(null, 1f, 2);
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            ScreenShakeFromAnimationCurve3D.Instance.ShakeCameraFromCurve();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            ScreenShakeFromAnimationCurve3D.Instance.ShakeCameraFromCurve(null, 1f, 2);
        }
    }
}
