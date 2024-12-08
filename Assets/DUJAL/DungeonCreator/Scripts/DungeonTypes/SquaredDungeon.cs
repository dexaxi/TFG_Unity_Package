namespace DUJAL.DungeonCreator
{ 
    using DungeonCreator.Types;
    using System.Collections.Generic;
    using UnityEngine;
    public class SquaredDungeon : Dungeon
    {
        public SquaredDungeon(int size)
        {
            if (size <= 0)
            { 
                Debug.LogError("Size must be greater than zero");
            }

            Size = size;
            _dungeonMatrix = new Dictionary<Vector2Int, List<Vector2Int>>(Size);
        
            for (int i = 0; i < Size; ++i)
            {
                for (int j = 0; j < Size; ++j)
                { 
                    _dungeonMatrix[new Vector2Int(i, j)] = new List<Vector2Int>();
                }
            }
        }

        // Generate function, starting position will be from where in the matrix the algoritm will start to generwate. Rooms is the total # of rooms and you can choose between Depth First Search and Prim to generate it.
        // PSA: Prim tends to look better with small dungoen "The Binding of Isaac" style. DFS Tends to look better if you're using this to generate bigger terrain like the "Stardew Valley"."
        public void Generate(Vector2Int startingPosition, int rooms, GenerationAlgorithm method = GenerationAlgorithm.DFS)
        {
            _isGenerated = true;
            switch (method)
            {
                case GenerationAlgorithm.Prim:
                    GeneratePrim(startingPosition, rooms);
                    break;

                case GenerationAlgorithm.DFS:
                    GenerateDFS(startingPosition, rooms);
                    break;
            }
        }

        // Returns tile description. int[,] is the coordinates. Assigns the value based on the number of adyacencies.
        public int[,] GetTileDescription()
        {
            int[,] tileDescription = new int[Size, Size];
            for (int x = 0; x < Size; ++x)
            {
                for (int y = 0; y < Size; ++y)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    tileDescription[x, y] = _dungeonMatrix[pos].Count;
                }
            }
            return tileDescription;
        }
    }
}
