namespace DUJAL.DungeonCreator.Types
{
    using UnityEngine;

    /// <summary>
    //  Scriptable Object that contains a reference to a room instance and it's data.
    /// </summary>
    [CreateAssetMenu(fileName = "New Dungeon Room Asset", menuName = "DUJAL/DungeonRoom")]
    public class DungeonRoom : ScriptableObject
    {
        public string Name;
        public GameObject RoomPrefab;
        public DirectionFlag Directions;
    }
}