namespace DUJAL.Systems.Dungeons
{
    using Dungeons.Types;
    using Dungeons.Utils;
    using System.Collections.Generic;
    using UnityEngine;

    public class Labyrinth : Dungeon
    {
        public Labyrinth(int size)
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

            _isGenerated = false;
        }

        public void Generate(Vector2Int startingPosition, GenerationAlgorithm method = GenerationAlgorithm.Prim)
        {
            _isGenerated = true;
            switch (method)
            {
                case GenerationAlgorithm.Prim:
                    GeneratePrim(startingPosition);
                    break;
                case GenerationAlgorithm.DFS:
                    GenerateDFS(startingPosition);
                    break;
            }
        }

        public int[,] GetTileDescription(out int descriptionSize)
        {
            if (!_isGenerated)
            {
                Debug.LogError("Error, Labyrinth not generated");
                descriptionSize = -1;
                return new int[0, 0];
            }

            descriptionSize = Size * 2 + 1;
            int[,] tileDescription = new int[descriptionSize, descriptionSize];

            for (int x = 0; x < Size; ++x)
            {
                for (int y = 0; y < Size; ++y)
                {
                    Vector2Int pos = new(x, y);
                    int parsedX = x * 2;
                    int parsedY = y * 2;

                    tileDescription[parsedX, parsedY] = 1;
                    tileDescription[parsedX, parsedY + 1] = 1;
                    tileDescription[parsedX + 1, parsedY] = 1;

                    if (_dungeonMatrix[pos].Count > 0)
                    {
                        tileDescription[parsedX + 1, parsedY + 1] = 0;
                    }
                    else
                    {
                        tileDescription[parsedX + 1, parsedX + 1] = 1;
                    }

                    if (x != 0 || y != 0)
                    {
                        foreach (Vector2Int adyacency in _dungeonMatrix[pos])
                        {
                            if (adyacency + Direction.East.Vector == pos)
                            {
                                tileDescription[parsedX, parsedY + 1] = 0;
                            }
                            else if (adyacency + Direction.North.Vector == pos)
                            {
                                tileDescription[parsedX + 1, parsedY] = 0;
                            }
                        }
                    }
                }
            }

            for (int x = 0; x < descriptionSize; ++x)
            {
                tileDescription[x, Size * 2] = 1;
            }
            for (int y = 0; y < descriptionSize; ++y)
            {
                tileDescription[Size * 2, y] = 1;
            }

            return tileDescription;
        }

        public void AddEdges(int edgesCount)
        {
            for (int _ = 0; _ < edgesCount; ++_)
            {
                Vector2Int randomPos = new(Random.Range(0, Size), Random.Range(0, Size));
                Direction randomDir = Direction.GetRandomDirection();
                Vector2Int newRoom = randomPos + randomDir.Vector;
                if (!HasSide(randomPos, randomDir) && DungeonUtils.IsValidDungeonPosition(newRoom, Size))
                {
                    _dungeonMatrix[randomPos].Add(newRoom);
                    _dungeonMatrix[newRoom].Add(randomPos);
                }
            }
        }
    }
}