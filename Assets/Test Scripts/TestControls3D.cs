using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DUJAL.IndependentComponents.ScreenShake;
using DUJAL.Systems.Audio;
using DUJAL.Systems.Saving;
using DUJAL.Systems.Utils;
using DUJAL.IndependentComponents.LaunchRigidBody;

public class TestControls3D : MonoBehaviour, ISaveData
{

    public Transform CameraParent;
    public Vector3 StartPos;
    public Vector3 EndPos;
    public bool Reverse;
    public int testInt;
    public SerializableDictionary<int, string> testDictionary;
    public List<string> testList;
    // Start is called before the first frame update
    void Start()
    {
        StartPos = CameraParent.position;
        EndPos = StartPos + new Vector3(0f, 0f, 5f);
        Reverse = false;
        testDictionary = new SerializableDictionary<int, string>();
        testList = new List<string>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            if (!ScreenShakeFromAnimationCurve3D.Instance.Shaking)
                    AudioManager.Instance.Play("thud");
            ScreenShakeFromAnimationCurve3D.Instance.ShakeScreen();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (!ScreenShakeFromAnimationCurve3D.Instance.Shaking)
                AudioManager.Instance.Play("pop");
            ScreenShakeFromAnimationCurve3D.Instance.ShakeScreen(AnimationCurveSelector.Instance.GetCurve(AnimationCurveType.Kick), 0.5f, 0.5f);
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            SceneManager.LoadScene(1);
        }
        if (Input.GetKeyDown(KeyCode.I)) 
        {
            testDictionary.Add(testInt, "TEST");
            testList.Add(testInt.ToString());
            testInt++;
            Debug.Log("testInt = " + testInt);
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            SaveDataHandler.Instance.SaveGame();
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            SaveDataHandler.Instance.LoadGame();
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("LaucnRigidBody triggered!");
            LaunchRigidBody.LaunchRigidBody3D(FindObjectOfType<Rigidbody>(), new Vector3(0, 1, 0), 10);
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            SaveDataHandler.Instance.SetCurrentGameSlot(SaveDataHandler.Instance.CurrentSlot - 1);
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            SaveDataHandler.Instance.SetCurrentGameSlot(SaveDataHandler.Instance.CurrentSlot + 1);
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
