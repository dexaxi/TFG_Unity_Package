using System.Collections.Generic;
using System.Linq;

namespace DUJAL.Systems.Saving
{
    using UnityEngine;

    /// <summary>
    //  Singleton Save Data Handler. Call this class to save or load your data.
    /// </summary>
    public class SaveDataHandler : MonoBehaviour
    {
        public static SaveDataHandler Instance { get; private set; }

        [Header("Save Data Storage Settings")]
        [SerializeField]
        [Tooltip("Name for the file that will contain the save data.")]
        private string fileName;

        [Space(10)]

        [SerializeField]
        [Tooltip("Activate this bool to encrypt the save data using XOR encryption")]
        private bool encryptSaveData;

        [Space(10)]

        [SerializeField]
        [Tooltip("If data encryption is enabled, this string will be used to encrypt and decript the data. WARNING: If this word is changed, save data MUST be regenerated." +
            " Important, save data is stored here: https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html.")]
        private string encryptionKey;

        private SaveData _saveData;
        private List<ISaveData> _saveDataInstances;
        private SaveDataFileHandler _saveFileHandler;
        public int CurrentSlot { get; private set; }

        private void Awake()
        {
            //Make Singleton
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }

            _saveDataInstances = FindAllSaveDataInstances();
            _saveData = new SaveData();
            _saveFileHandler = new SaveDataFileHandler(Application.persistentDataPath, fileName, encryptSaveData, encryptionKey);
        }

        private void Start()
        {
            //Load save data from file
            LoadGame();
        }

        /// <summary>
        //  Function to save data to file. WARNING: WILL OVEWRITE CURRENT SAVED DATA
        /// </summary>
        public void SaveGame()
        {
            if (_saveDataInstances != null)
            {
                foreach (ISaveData data in _saveDataInstances)
                {
                    SaveData currentSlotSaveData = _saveData;
                    data.SaveData(ref currentSlotSaveData);
                }
                _saveFileHandler.Save(_saveData);
            }
            else
            {
                Debug.LogWarning("SaveDataHandler: No ISaveData Instances Found.");
            }
        }

        /// <summary>
        //  Function to load game.
        /// </summary>
        public void LoadGame()
        {
            _saveData = _saveFileHandler.Load();

            if (_saveData == null)
            {
                this._saveData = new SaveData();
            }

            if (_saveDataInstances != null)
            {
                foreach (ISaveData data in _saveDataInstances)
                {
                    data.LoadData(_saveData);
                }
            }
            else
            {
                Debug.LogWarning("SaveDataHandler: No ISaveData Instances Found.");
            }
        }

        /// <summary>
        //  Function to delete current slot save data.
        /// </summary>
        public void ResetCurrentSlot()
        {
            _saveData.SaveSlots[CurrentSlot] = new SaveSlot();
            _saveFileHandler.Save(_saveData);
            LoadGame();
        }

        /// <summary>
        //  Function to delete all of the save data.
        /// </summary>
        public void ResetSave()
        {
            _saveData = new SaveData();
            _saveFileHandler.Save(_saveData);
            CurrentSlot = 0;
            SaveGame();
            LoadGame();
        }


        /// <summary>
        //  Function to delete one slot from @param.
        /// </summary>
        public void DeleteSlot(int slot)
        {
            if (_saveData.SaveSlots.Count == 1)
            {
                CurrentSlot = 0;
                ResetCurrentSlot();
                return;
            }
            if (slot < _saveData.SaveSlots.Count && slot >= 0)
            {
                if (slot != CurrentSlot)
                {
                    _saveData.RemoveSlot(slot);
                    if (slot < CurrentSlot) CurrentSlot--;
                    SaveGame();
                    LoadGame();
                }
                else
                {
                    if ((slot - 1) >= 0)
                    {
                        SetCurrentGameSlot(CurrentSlot - 1);
                        _saveData.RemoveSlot(slot);
                        SaveGame();
                        LoadGame();
                    }
                    else if ((slot + 1) < _saveData.SaveSlots.Count)
                    {
                        SetCurrentGameSlot(CurrentSlot + 1);
                        _saveData.RemoveSlot(slot);
                        if (CurrentSlot >= _saveData.SaveSlots.Count) CurrentSlot--;
                        SaveGame();
                        LoadGame();
                    }
                }
            }
        }

        /// <summary>
        //  Function to Save the current slot and make a copy of it.
        /// </summary>
        public void SaveSlotAndCopyToNew()
        {
            SaveGame();
            LoadGame();
            _saveData.AddSlot();
            CurrentSlot = _saveData.SaveSlots.Count - 1;
            SaveGame();
        }

        /// <summary>
        //  Function to save the current pending data to a new slot
        /// </summary>
        public void SaveOnNewSlot()
        {
            _saveData.AddSlot();
            CurrentSlot = _saveData.SaveSlots.Count - 1;
            SaveGame();
        }

        /// <summary>
        //  Function to create a new, empty save slot
        /// </summary>
        public void CreateNewGameSlot()
        {
            LoadGame();
            _saveData.AddSlot();
            CurrentSlot = _saveData.SaveSlots.Count - 1;
            ResetCurrentSlot();
            SaveGame();
        }

        /// <summary>
        //  Function to change the current game slot
        /// </summary>
        public void SetCurrentGameSlot(int slot)
        {
            if (slot < _saveData.SaveSlots.Count && slot >= 0)
            {
                CurrentSlot = slot;
                LoadGame();
            }
        }

        /// <summary>
        //  Enumerable to get all save data interface instances.
        /// </summary>
        private List<ISaveData> FindAllSaveDataInstances()
        {
            IEnumerable<ISaveData> saveDataInstances = FindObjectsOfType<MonoBehaviour>().OfType<ISaveData>();
            return new List<ISaveData>(saveDataInstances);
        }

        /// <summary>
        //  Debug function to save the game.
        /// </summary>
        [ContextMenu("SaveGame")]
        public void DebugSaveGame()
        {
            SaveGame();
        }

        /// <summary>
        //  Debug function to load the game.
        /// </summary>
        [ContextMenu("LoadGame")]
        public void DebugLoadGame()
        {
            LoadGame();
        }

        /// <summary>
        //  Debug function to Reset the current save slot.
        /// </summary>
        [ContextMenu("ResetCurrentSlot")]
        public void DebugResetCurrentSlot()
        {
            ResetCurrentSlot();
        }

        /// <summary>
        //  Debug function reset the save file.
        /// </summary>
        [ContextMenu("ResetSave")]
        public void DebugResetSave()
        {
            ResetSave();
        }

        /// <summary>
        //  Debug function to save the current save slot and copy it to a new one.
        /// </summary>
        [ContextMenu("SaveSlotAndCopyToNew")]
        public void DebugSaveSlotAndCopyToNew()
        {
            SaveSlotAndCopyToNew();
        }

        /// <summary>
        //  Debug function to save the current progress on a new save slot.
        /// </summary>
        [ContextMenu("SaveOnNewSlot")]
        public void DebugSaveOnNewSlot()
        {
            SaveOnNewSlot();
        }

        /// <summary>
        //  Debug function to create a new blank save slot.
        /// </summary>
        [ContextMenu("CreateNewGameSlot")]
        public void DebugCreateNewGameSlot()
        {
            CreateNewGameSlot();
        }

        /// <summary>
        //  Debug function to delete a slot.
        /// </summary>
        [ContextMenu("DeleteCurrentSlot")]
        public void DebugDeleteCurrentSlot()
        {
            DeleteSlot(CurrentSlot);
        }
    }
}
