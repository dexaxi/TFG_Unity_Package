namespace DUJAL.DungeonCreator
{ 
    using UnityEngine;

    public class DungeonGenerator : MonoBehaviour
    {
        public int Rooms;

        private SquaredDungeon _squaredDungeon;
        
        private void Start()
        {
        }

        public void GeneratePrimDungeon() 
        {
            int dungeonSize = Rooms * Rooms;
            int centerCoord = dungeonSize / 2;
            Vector2Int startingPos = new(centerCoord, centerCoord);
            _squaredDungeon = new(dungeonSize);
            _squaredDungeon.Generate(startingPos, Rooms, Types.GenerationAlgorithm.Prim);
        }
    }
}
