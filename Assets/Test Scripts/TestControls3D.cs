using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DUJAL.IndependentComponents.ScreenShake;
using DUJAL.Systems.Audio;
using DUJAL.Systems.Saving;
using DUJAL.Systems.Utils;
using DUJAL.IndependentComponents.LaunchRigidBody;
using DUJAL.MovementComponents.PhysicsBased3D;
using Cinemachine;
public class TestControls3D : MonoBehaviour, ISaveData
{

    public Vector3 StartPos;
    public Vector3 EndPos;
    public bool Reverse;
    public int testInt;
    public SerializableDictionary<int, string> testDictionary;
    public List<string> testList;
    PhysicsBasedThirdPersonMovement3D tps;
    // Start is called before the first frame update
    void Start()
    {
        EndPos = StartPos + new Vector3(0f, 0f, 5f);
        Reverse = false;
        testDictionary = new SerializableDictionary<int, string>();
        tps = FindObjectOfType<PhysicsBasedThirdPersonMovement3D>();
        testList = new List<string>();
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.O))
        {
            if (!ScreenShakeFromAnimationCurve3D.Instance.Shaking)
                AudioManager.Instance.Play("thud");
            tps.DebugRotateIsometricCamera90Degrees();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (!ScreenShakeFromAnimationCurve3D.Instance.Shaking)
                AudioManager.Instance.Play("pop");
            CinemachineFreeLook camera = tps.CurrentCamera.GetComponentInChildren<CinemachineFreeLook>();
            //ScreenShakeFromAnimationCurve3D.Instance.ShakeScreenCinemachine(camera, AnimationCurveSelector.Instance.GetCurve(AnimationCurveType.Kick));
            ScreenShakeFromAnimationCurve3D.Instance.ShakeScreenCinemachine(camera, null);

        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            SceneManager.LoadScene(1);
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (tps != null) tps.ChangeCameraMode(CameraMode.CameraMode_Normal);
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            if (tps != null) tps.ChangeCameraMode(CameraMode.CameraMode_Shoulder);
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            if (tps != null) tps.ChangeCameraMode(CameraMode.CameraMode_TopDown);
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (tps != null) tps.ChangeCameraMode(CameraMode.CameraMode_FixedYTopDown);
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (tps != null) tps.ChangeCameraMode(CameraMode.CameraMode_FixedXYTopDown);
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (tps != null) tps.ChangeCameraMode(CameraMode.CameraMode_Isometric);
        }
        if (Input.GetKeyUp(KeyCode.Alpha0))
        {
            SaveDataHandler.Instance.DeleteSlot(0);
        }
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            SaveDataHandler.Instance.DeleteSlot(1);
        }
        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            SaveDataHandler.Instance.DeleteSlot(2);
        }
        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            SaveDataHandler.Instance.DeleteSlot(3);
        }
    }

    public void LoadData(SaveData data) 
    {
        this.testInt = data.SaveSlots[SaveDataHandler.Instance.CurrentSlot].TestSavedInt;
        data.SaveSlots[SaveDataHandler.Instance.CurrentSlot].TestDictionary.CopySerializableDictionary(this.testDictionary);
        this.testList = new List<string>(data.SaveSlots[SaveDataHandler.Instance.CurrentSlot].TestList);
    }
    public void SaveData(ref SaveData data) 
    {
        data.SaveSlots[SaveDataHandler.Instance.CurrentSlot].TestSavedInt = this.testInt;
        testDictionary.CopySerializableDictionary(data.SaveSlots[SaveDataHandler.Instance.CurrentSlot].TestDictionary);
        data.SaveSlots[SaveDataHandler.Instance.CurrentSlot].TestList = new List<string>(this.testList);
    }

}
