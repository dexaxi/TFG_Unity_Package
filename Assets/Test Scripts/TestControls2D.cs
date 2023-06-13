using UnityEngine;
using UnityEngine.SceneManagement;
using DUJAL.IndependentComponents.ScreenShake;
using DUJAL.IndependentComponents.LaunchRigidBody;
using DUJAL.MovementComponents.PhysicsBased2D;

public class TestControls2D : MonoBehaviour
{

    public Vector3 StartPos;
    public Vector3 EndPos;
    public bool Reverse;

    // Start is called before the first frame update
    void Start()
    {
        EndPos = StartPos + new Vector3(5f, 0f, 0f);
        Reverse = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O)) 
        {
            ScreenShakeFromAnimationCurve2D.Instance.ShakeScreen();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            ScreenShakeFromAnimationCurve2D.Instance.ShakeScreen(null, 0.5f, 0.5f);
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            SceneManager.LoadScene(0);
        }
        if (Input.GetKeyDown(KeyCode.T)) 
        {
            LaunchRigidBody.LaunchRigidBody2D(FindObjectOfType<Rigidbody2D>(), new Vector2(0, 1), 10);
        }

        /*if (!Reverse)
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
        }*/
    }
}
