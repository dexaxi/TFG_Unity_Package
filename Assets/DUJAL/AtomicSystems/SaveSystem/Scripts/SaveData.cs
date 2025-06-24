
namespace DUJAL.Systems.Saving
{
    using DUJAL.Systems.Utils;
    using System.Collections.Generic;
    using UnityEngine;

    [System.Serializable]
    public class SaveSlot
    {
        //Add here AS PUBLIC VARIABLES all the data that you want to save. Delete the Test Dictionary and Int if necessary.
        public int TestSavedInt;
        public SerializableDictionary<int, string> TestDictionary;
        public List<string> TestList;
        public SaveSlot()
        {
            //Default constructor for Save Data (assign default values at the start of the game FOR ALL THE VARIABLES YOU WANT TO SAVE). 
            TestSavedInt = 0;
            TestDictionary = new SerializableDictionary<int, string>();
            TestList = new List<string>();
        }
    }

    /// <summary>
    //  Serializable Class that contains the Saved Data that will be saved to the JSON file.
    /// </summary>
    [System.Serializable]
    public class SaveData
    {
        [Header("Savable data")]
        public List<SaveSlot> SaveSlots;
        public SaveData()
        {
            SaveSlots = new List<SaveSlot>();
            AddSlot();
        }

        public void AddSlot()
        {
            SaveSlots.Add(new SaveSlot());
        }

        public void RemoveSlot(int slot)
        {
            SaveSlots.Remove(SaveSlots[slot]);
        }
    }
}
