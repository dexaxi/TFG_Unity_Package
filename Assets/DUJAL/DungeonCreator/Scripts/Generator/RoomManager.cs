namespace DUJAL.DungeonCreator
{
    using System.Collections.Generic;
    using UnityEngine;

    public class RoomManager : MonoBehaviour
    {
        public static RoomManager Instance { get; private set; }

        public List<RoomInstance> Rooms = new();

        private void Awake()
        {
            if (Instance != null) 
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void ToggleDoors() 
        {
            //Debug.Log($"Toggling {Rooms.Count} rooms... ");
            foreach (RoomInstance room in Rooms) 
            { 
                room.ToggleDoors();  
            }
        }
    }

}