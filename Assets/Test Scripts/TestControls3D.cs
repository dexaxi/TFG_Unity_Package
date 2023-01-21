using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class TestControls3D : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            ScreenShakeFromAnimationCurve3D.Instance.ShakeScreen();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            ScreenShakeFromAnimationCurve3D.Instance.ShakeScreen(null, 0.5f, 0.5f);
        }
        if(Input.GetKeyDown(KeyCode.L))
        {
            SceneManager.LoadScene(1);
        }
    }
}
