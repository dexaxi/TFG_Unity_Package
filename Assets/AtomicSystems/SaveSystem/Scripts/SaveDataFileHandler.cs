using System;
using System.IO;

namespace DUJAL.Systems.Saving
{
    using UnityEngine;
    /// <summary>
    //  Saved Data File Handler Class.
    /// </summary>
    public class SaveDataFileHandler
    {
        private string _dirPath;
        private string _fileName;
        private bool _encryptSaveData;
        private string _encryptionKey;
        public SaveDataFileHandler(string dirPath, string fileName, bool encryptSaveData, string encryptionKey)
        {
            _dirPath = dirPath;
            _fileName = fileName;
            _encryptSaveData = encryptSaveData;
            _encryptionKey = encryptionKey;
        }

        /// <summary>
        //  Deserialize data from json.
        /// </summary>
        public SaveData Load()
        {
            string fullPath = Path.Combine(_dirPath, _fileName);
            SaveData data = null;
            if (File.Exists(fullPath))
            {
                try
                {
                    string jsonSaveData = "";
                    using (FileStream newStream = new FileStream(fullPath, FileMode.Open))
                    {
                        using (StreamReader newReader = new StreamReader(newStream))
                        {
                            jsonSaveData = newReader.ReadToEnd();
                        }
                    }

                    if (_encryptSaveData) jsonSaveData = EncryptDecrypt(jsonSaveData);

                    data = JsonUtility.FromJson<SaveData>(jsonSaveData);
                }
                catch (Exception e)
                {
                    Debug.LogError("SaveDataFileHandler: ERROR OCURRED LOADING DATA - Could not load data from: " + fullPath + "\n" + e);
                }
            }
            return data;
        }

        /// <summary>
        //  Serialize data to json.
        /// </summary>
        public void Save(SaveData data)
        {
            string fullPath = Path.Combine(_dirPath, _fileName);
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

                string jsonSaveData = JsonUtility.ToJson(data, true);

                if (_encryptSaveData) jsonSaveData = EncryptDecrypt(jsonSaveData);

                using (FileStream newStream = new FileStream(fullPath, FileMode.Create))
                {
                    using (StreamWriter newWriter = new StreamWriter(newStream))
                    {
                        newWriter.Write(jsonSaveData);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("SaveDataFileHandler: ERROR OCURRED SAVING DATA - Could not save data to: " + fullPath + "\n" + e);
            }
        }

        /// <summary>
        //  Function to decrypt data using a simple XOR algorithm.
        /// </summary>
        private string EncryptDecrypt(string data)
        {
            string processedData = "";
            for (int i = 0; i < data.Length; i++)
            {
                processedData += (char)(data[i] ^ _encryptionKey[i % _encryptionKey.Length]);
            }
            return processedData;
        }
    }
}
