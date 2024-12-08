namespace DUJAL.DungeonCreator
{
    using DungeonCreator.Types;

    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class RoomInstance : MonoBehaviour
    {
        public DirectionFlag DirectionFlag;
        public List<Door> Doors = new();

        private void Awake()
        {
            if (Doors.Count == 0) 
            {
                Doors = GetComponentsInChildren<Door>().ToList();
            }
        }

        public void ToggleDoors()
        {
            //Debug.Log($"Toggling {Doors.Count} doors...");
            foreach (var door in Doors)
            {
                bool isDoorEnabled = DirectionFlag.IsDirectionEnabled(door.Direction);
                //Debug.Log($"Door: + {door.name} isEnabled: {isDoorEnabled}, Directions: {DirectionFlag}");
                door.ToggleDoor(!isDoorEnabled);
            }
        }
    }

}