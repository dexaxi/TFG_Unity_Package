using DUJAL.IndependentComponents.ScreenShake;
using DUJAL.Systems.Audio;
using DUJAL.Systems.Saving;
using DUJAL.Systems.Utils;
using DUJAL.MovementComponents.PhysicsBased3D;
using DUJAL.MovementComponents.DiscreteBased3D;

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    ThirdPersonMovement3D tpsd;
    PhysicsBasedFirstPersonMovement3D fps;
    FirstPersonMovement3D fpsd;

    // Start is called before the first frame update
    void Start()
    {
        EndPos = StartPos + new Vector3(0f, 0f, 5f);
        Reverse = false;
        testDictionary = new SerializableDictionary<int, string>();
        tps = FindObjectOfType<PhysicsBasedThirdPersonMovement3D>();
        tpsd = FindObjectOfType<ThirdPersonMovement3D>();
        fps = FindObjectOfType<PhysicsBasedFirstPersonMovement3D>();
        fpsd = FindObjectOfType<FirstPersonMovement3D>();
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
            { 
                AudioManager.Instance.Play("pop");
            }
            CinemachineFreeLook cinemachineCamera = GetCinemachineCamera();
            Camera camera = GetCamera();
            
            if (cinemachineCamera != null) 
            {
                ScreenShakeFromAnimationCurve3D.Instance.ShakeScreenCinemachine(cinemachineCamera, null);
            }
            if (camera != null) 
            {
                ScreenShakeFromAnimationCurve3D.Instance.ShakeScreen();
            }
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
    }

    private CinemachineFreeLook GetCinemachineCamera() 
    {
        if (tps != null) return tps.CurrentCamera.GetComponentInChildren<CinemachineFreeLook>();
        if (tpsd != null) return tpsd.CurrentCamera.GetComponentInChildren<CinemachineFreeLook>();
        return null;
    }
    private Camera GetCamera() 
    {
        if (fps != null) return fps.Camera;
        if (fpsd != null) return fpsd.Camera;
        return null;
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
