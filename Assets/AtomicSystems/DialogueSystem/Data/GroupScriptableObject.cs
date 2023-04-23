namespace DUJAL.Systems.Dialogue
{
    using UnityEngine;

    public class GroupScriptableObject : ScriptableObject
    {
        [field: SerializeField] public string GroupName { get; set; }
        public void Initialize(string groupName) 
        {
            GroupName = groupName;
        }

    }
}
