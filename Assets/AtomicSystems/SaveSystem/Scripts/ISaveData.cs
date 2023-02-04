using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DUJAL.Systems.Saving
{
    /// <summary>
    //  Simple save data interface. Implement this interfact and its functions to define how loading and saving data work in each component.
    /// </summary>
    public interface ISaveData
    {
        void LoadData(SaveData data);
        void SaveData(ref SaveData data);
    }
}
