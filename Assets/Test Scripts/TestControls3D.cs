using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class TestControls3D : MonoBehaviour
{

    public Transform CameraParent;
    public Vector3 StartPos;
    public Vector3 EndPos;
    public bool Reverse;
    // Start is called before the first frame update
    void Start()
    {
        StartPos = CameraParent.position;
        EndPos = StartPos + new Vector3(0f, 0f, 5f);
        Reverse = false;
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
            ScreenShakeFromAnimationCurve3D.Instance.ShakeScreen(null, 0.5f, 1f);
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            SceneManager.LoadScene(1);
        }

        //CameraParent.rotation = Quaternion.Euler(new Vector3(CameraParent.rotation.eulerAngles.x, CameraParent.rotation.eulerAngles.y + 60f * Time.deltaTime, CameraParent.rotation.eulerAngles.z));
        if (!Reverse)
        {
            CameraParent.position = Vector3.MoveTowards(CameraParent.position, EndPos, 5f * Time.deltaTime);
            if (Vector3.Distance(CameraParent.position, EndPos) == 0)
            {
                Reverse = true;
            }
        }
        else
        {
            CameraParent.position = Vector3.MoveTowards(CameraParent.position, StartPos, 5f * Time.deltaTime);
            if (Vector3.Distance(CameraParent.position, StartPos) == 0)
            {
                Reverse = false;
            }
        }


    }

}
