using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DUJAL.Systems.Dialogue
{
    public class GroupScriptableObject : ScriptableObject
    {
        [field: SerializeField] public string GroupName { get; set; }
        public void Initialize(string groupName) 
        {
            GroupName = groupName;
        }

    }
}
