namespace DUJAL.DungeonCreator
{
    using DungeonCreator.Types;
    using DungeonCreator.Constants;

    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using DUJAL.DungeonCreator.Utils;

    public class DungeonGenerator : MonoBehaviour
    {
        public int Rooms;
        
        public Vector2Int RoomSize;

        public Vector2Int StartingCell = Vector2Int.zero;

        private SquaredDungeon _squaredDungeon;

        private List<DungeonRoom> _rooms = new();

        private readonly Dictionary<DirectionFlag, List<DungeonRoom>> _roomPermutations = new();

        // All possible direction permutations. To fill with different rooms.
        private static readonly DirectionFlag[] _permutations =
        {
            new (false, false, false, false),   //N/A  -1, 
            
            new (true, false, false, false),    //W     1
            new (false, true, false, false),    //E     2
            new (false, false, true, false),    //N     3
            new (false, false, false, true),    //S     4
            
            new (true, true, false, false),     //WE    5
            new (true, false, true, false),     //WN    6
            new (true, false, false, true),     //WS    7
            new (false, true, true, false),     //EN    8
            new (false, true, false, true),     //ES    9
            new (false, false, true, true),     //NS    10
            
            new (true, true, true, false),      //WEN   11
            new (true, true, false, true),      //WES   12
            new (true, false, true, true),      //WNS   13
            new (false, true, true, true),      //ENS   14
            
            new (true, true, true, true),       //WENS  15
        };

        private void Start()
        {
            var objArray = Resources.LoadAll(DungeonConstants.DungeonRoomsPath, typeof(DungeonRoom));
            if (objArray == null || objArray.Length == 0)
            {
                Debug.LogError("Error, no room scriptable objects found");
                return;
            }
            
            _rooms = objArray.Cast<DungeonRoom>().ToList();
            PrecalculateRoomPermutations();

            GeneratePrimDungeon();
        }

        // Load all dungeon rooms from the Resources path. Then populate the dictionary with the relevant data.
        private void PrecalculateRoomPermutations() 
        {
            foreach (DirectionFlag permutation in _permutations)
            {
                _roomPermutations[permutation] = new();
            }

            foreach (DungeonRoom room in _rooms) 
            {
                _roomPermutations[room.Directions].Add(room);
            }
        }

        // Generate prim dungeon based on a Room Size,starting cell and a # of Rooms.
        public void GeneratePrimDungeon()
        {
            int dungeonSize = GetDungeonSize();
            
            // Instantiate dungeon size Rooms * Rooms
            _squaredDungeon = new(dungeonSize);
            
            // Generate using prim, starting cell and rooms
            _squaredDungeon.Generate(StartingCell, Rooms, GenerationAlgorithm.Prim);
            
            var tiles = _squaredDungeon.GetTileDescription();

            for (int x = 0; x < _squaredDungeon.Size; x++) 
            {
                for (int y = 0; y < _squaredDungeon.Size; y++) 
                {
                    // If theres an adyacency we instantiate a tile.
                    // We could check for "boss rooms" or "exit tiles" by checking for tiles[x,y] == 1. To pre get all of the possible exit tiles we could to _squaredDungeon.GetLeaves();
                    if (tiles[x,y] != 0)
                    {
                        Vector2Int currentPos = new(x,y);
                        // We try to find a possible room to instantiate (randomly from the loaded room list)
                        var roomToInstantiate = GetRandomRoomToInstantiate(currentPos);
                        if (roomToInstantiate != null)
                        {
                            //Debug.Log("Instantiating room at x:" + x + " y: " + y + " Directions: " + roomToInstantiate.Directions);
                            Vector3 instantiationPosition = new(x * RoomSize.x, 0, y * RoomSize.y);
                            var roomGO = Instantiate(roomToInstantiate.RoomPrefab, instantiationPosition, Quaternion.identity, transform);
                            roomGO.name = $"{roomToInstantiate.Name} (x: {x}, y: {y})";

                            var roomInstance = roomGO.AddComponent<RoomInstance>();
                            roomInstance.DirectionFlag = roomToInstantiate.Directions;
                            RoomManager.Instance.Rooms.Add(roomInstance);
                        }
                        else 
                        {
                            var errorRoom = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            errorRoom.transform.position = new Vector3(x * RoomSize.x, 0, y * RoomSize.y);
                            errorRoom.name = $"Error: (x: {x}, y: {y})";
                            Debug.LogError("Error: Should always finda room to instantiate");
                        }
                    }
                }
            }

            OnRoomsInstantiated();
        }

        //Here we should add the rest of the logic needed to generate dungeons.
        private void OnRoomsInstantiated()
        {
            Debug.Log("Dungeon generated! Invoking RoomHandler...");
            RoomManager.Instance.ToggleDoors();
        }


        private DungeonRoom GetRandomRoomToInstantiate(Vector2Int currentPos) 
        {
            List<Direction> availableDoors = new();
            
            for (Direction dir = 0; dir < Direction.Count; dir++) 
            {
                if (_squaredDungeon.HasSide(currentPos, dir))
                { 
                    availableDoors.Add(dir);
                }
            }
            
            DirectionFlag directionPermutation = new (availableDoors);

            if (availableDoors.Count == 0 || !directionPermutation.IsValid()) 
            {
                return null; 
            }
            
            // We get all of the possible rooms given the possible adyacencies. 
            List<DungeonRoom> validRooms = _roomPermutations[directionPermutation];
            
            // We return a random valid room. Bear in mind we could add rarities to the different rooms by adding a "Weight" parameter to DungeonRoom.cs.
            // Then use the GetRandomElement(list, weightList) variant from ListUtil.
            return ListUtils<DungeonRoom>.GetRandomElement(validRooms);
        }

        private int GetDungeonSize() 
        {
            return Rooms * 2;
        }

        public void SetStartingCellToMiddleCell() 
        {
            int dungeonSize = GetDungeonSize();
            int centerCoord = (int) (dungeonSize / 2.0f);
            StartingCell = new(centerCoord, centerCoord);
        }
    }
}
