namespace DUJAL.Systems.Dungeons 
{
    using Dungeons.Utils;
    using Dungeons.Types;
    using System.Collections.Generic;
    using UnityEngine;

    public class Dungeon
    {
        public int Size { get; protected set; }
        protected Dictionary<Vector2Int, List<Vector2Int>> _dungeonMatrix;
        protected bool _isGenerated;

        private Direction[,,] GenerateDFSDirectionGrid()
        {
            Direction[,,] directionGrid = new Direction[Size, Size, 4];

            List<Direction> possibleDirections = new();
            for (Direction dir = 0; dir < Direction.Count; ++dir)
            {
                possibleDirections.Add(dir);
            }

            for (int x = 0; x < Size; ++x)
            {
                for (int y = 0; y < Size; ++y)
                {
                    List<Direction> randomDirections = ListUtils<Direction>.Shuffle(possibleDirections);
                    possibleDirections = new List<Direction>();
                    for (Direction dir = 0; dir < Direction.Count; ++dir)
                    {
                        possibleDirections.Add(dir);
                    }

                    for (Direction dir = 0; dir < Direction.Count; ++dir)
                    {
                        directionGrid[x, y, dir] = randomDirections[dir];
                    }
                }
            }
            return directionGrid;
        }

        protected void GenerateDFS(Vector2Int startingPos, int rooms = int.MaxValue)
        {
            int roomIdx = 0;
            Direction[,,] directionGrid = GenerateDFSDirectionGrid();

            bool[,] visited = new bool[Size, Size];

            Stack<int> roomStack = new();
            roomStack.Push(DungeonUtils.Translate(startingPos, Size));

            visited[startingPos.x, startingPos.y] = true;
            while (roomStack.Count > 0 && roomIdx < rooms)
            {
                Vector2Int roomPos = DungeonUtils.Translate(roomStack.Pop(), Size);
                for (Direction dir = 0; dir < Direction.Count; ++dir)
                {
                    Vector2Int tentativePos = DungeonUtils.SumDir(directionGrid[roomPos.x, roomPos.y, dir], roomPos, Size);
                    if (tentativePos != DungeonUtils.InvalidVector && !visited[tentativePos.x, tentativePos.y] && roomIdx < rooms)
                    {
                        visited[tentativePos.x, tentativePos.y] = true;
                        roomStack.Push(DungeonUtils.Translate(tentativePos, Size));
                        _dungeonMatrix[roomPos].Add(tentativePos);
                        _dungeonMatrix[tentativePos].Add(roomPos);
                        ++roomIdx;
                    }
                }
            }
        }

        protected void GeneratePrim(Vector2Int startingPos, int rooms = int.MaxValue)
        {
            PriorityQueue<Edge> edges = new();
            int num = 0;

            bool[,] visited = new bool[Size, Size];
            edges.Enqueue(new Edge(startingPos, startingPos + Direction.GetRandomDirection(), Random.Range(0, 10000)));
            edges.Enqueue(new Edge(startingPos, startingPos + Direction.GetRandomDirection(), Random.Range(0, 10000)));

            visited[startingPos.x, startingPos.y] = true;
            while (edges.Count() > 0 && num < rooms)
            {

                Edge edge = edges.Dequeue();
                Vector2Int end = edge.end;
                Vector2Int origin = edge.origin;

                if (!visited[end.x, end.y])
                {
                    visited[end.x, end.y] = true;
                    _dungeonMatrix[end].Add(origin);
                    _dungeonMatrix[origin].Add(end);
                    ++num;

                    for (Direction dir = 0; dir < Direction.Count; ++dir)
                    {
                        Vector2Int newPos = end + dir.Vector;
                        if (DungeonUtils.IsValidDungeonPosition(newPos, Size))
                        {
                            edges.Enqueue(new Edge(end, newPos, Random.Range(0, 10000)));
                        }
                    }
                }
            }
        }
        public List<Vector2Int> GetLeaves()
        {
            if (!_isGenerated)
            {
                Debug.LogError("Labyrinth not generated");
                return new List<Vector2Int>();
            }

            List<Vector2Int> leaves = new();

            for (int x = 0; x < Size; ++x)
            {
                for (int y = 0; y < Size; ++y)
                {
                    Vector2Int tentativeLeave = new(x, y);
                    if (_dungeonMatrix[tentativeLeave].Count == 1)
                    {
                        leaves.Add(tentativeLeave);
                    }
                }
            }
            return leaves;
        }

        public Graph GetGraphDescription()
        {
            if (!_isGenerated)
            {
                Debug.LogError("Dungeon not generated.");
                return new Graph(0);
            }
            Graph graphDescription = new(Size * Size);

            for (int x = 0; x < Size; ++x)
            {
                for (int y = 0; y < Size; ++y)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    foreach (Vector2Int adyacency in _dungeonMatrix[pos])
                    {
                        int destination = DungeonUtils.Translate(adyacency, Size);
                        graphDescription.AddEdge(DungeonUtils.Translate(pos, Size), destination);
                    }
                }
            }
            return graphDescription;
        }

        public Vector2Int GetPositionFromGraphAdyacency(int vertex) 
        {
            return DungeonUtils.Translate(vertex, Size);
        }

        public bool HasNorth(Vector2Int position)
        {
            return HasSide(position, Direction.North);
        }
        public bool HasEast(Vector2Int position)
        {
            return HasSide(position, Direction.East);
        }
        public bool HasSouth(Vector2Int position)
        {
            return HasSide(position, Direction.South);
        }
        public bool HasWest(Vector2Int position)
        {
            return HasSide(position, Direction.West);
        }

        public bool HasSide(Vector2Int position, Direction direction)
        {
            if (!DungeonUtils.IsValidDungeonPosition(position, Size))
            {
                return false;
            }

            bool hasSide = false;
            foreach (Vector2Int cellAdyacency in _dungeonMatrix[position])
            {
                hasSide = hasSide || cellAdyacency == position + direction.Vector;
            }
            return hasSide;
        }

    }
}
