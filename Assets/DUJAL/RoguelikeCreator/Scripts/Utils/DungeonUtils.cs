namespace DUJAL.Systems.Dungeons.Utils 
{
    using Dungeons.Types;
    using UnityEngine;

    public static class DungeonUtils
    {
        public static Vector2Int InvalidVector { get; private set; } = new Vector2Int(int.MaxValue, int.MaxValue);

        public static bool IsValidDungeonPosition(this Vector2Int position, int size) => position.x >= 0 && position.x < size && position.y >= 0 && position.y < size;

        public static Vector2Int Translate(int number, int size) => new(number / size, number % size);

        public static int Translate(Vector2Int position, int size) => position.x * size + position.y;
       
        public static Vector2Int SumDir(Direction direction, Vector2Int position, int size)
        {
            position += direction.Vector;
            return IsValidDungeonPosition(position, size) ? position : InvalidVector;
        }

    }
}
