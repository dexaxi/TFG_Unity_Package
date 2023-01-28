using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class UITest : MonoBehaviour
{
    public TextMeshProUGUI slot;
    public TextMeshProUGUI intTest;
    public TextMeshProUGUI dictCount;
    private int sloter;
    private void Start()
    {
        sloter = 0;
    }
    private void Update()
    {
        intTest.text = FindObjectOfType<TestControls3D>().testInt.ToString() ;
        dictCount.text = FindObjectOfType<TestControls3D>().testDictionary.Keys.Count.ToString();
        slot.text = SaveDataHandler.Instance.CurrentSlot.ToString() ;
    }

    public void SetSlotPlus() 
    {
        SaveDataHandler.Instance.SetCurrentGameSlot(sloter++);
    }
    public void SetSlotMinus()
    {
        SaveDataHandler.Instance.SetCurrentGameSlot(sloter--);
    }
    public void SaveGame() { SaveDataHandler.Instance.SaveGame(); }
    public void LoadGame() { SaveDataHandler.Instance.LoadGame(); }
}
