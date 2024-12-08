namespace DUJAL.DungeonCreator.Types
{
    using UnityEngine;

    [System.Serializable]
    public class Direction
    {
        public static int Count => (int) CardinalDirection.Count;
        public static Direction FirstDir  = CardinalDirection.FirstDir;
        public static Direction LastDir   = CardinalDirection.LastDir;
        
        [Tooltip("(0, 0)")]
        public static Direction Invalid   = CardinalDirection.Invalid;
        [Tooltip("(-1, 0)")]
        public static Direction West      = CardinalDirection.West;
        [Tooltip("(1, 0)")]
        public static Direction East      = CardinalDirection.East;
        [Tooltip("(0 ,1)")]
        public static Direction North     = CardinalDirection.North;
        [Tooltip("(0, -1)")]
        public static Direction South     = CardinalDirection.South;

        public CardinalDirection Value { get; private set; } = CardinalDirection.Invalid;
        public Vector2Int Vector => GetDirection();

        private static Vector2Int _westVec =  new  ( -1,   0  );
        private static Vector2Int _eastVec =  new  (  1,   0  );
        private static Vector2Int _northVec = new  (  0,   1  );
        private static Vector2Int _southVec = new  (  0,  -1  );
        private static Vector2Int _noDirVec = new  (  0,   0  );

        public Direction(CardinalDirection direction) => Value = direction;

        public Direction(int direction) => Value = (CardinalDirection) direction;

        public Direction(Vector2Int direction) 
        {
            if (direction == _westVec) Value = West;
            else if (direction == _eastVec) Value = East;
            else if (direction == _northVec) Value = North;
            else if (direction == _southVec) Value = South;
            else 
            {
                Debug.LogError("Error: Trying to initialize direction with an invalid vector (" + direction.x + ", " + direction.y + ").");
                Value = Invalid;
            }
        }

        public enum CardinalDirection
        {
            Invalid = -1,
            FirstDir = 0,
        
            West = FirstDir,
            East = 1,
            North = 2,
            South = 3,
        
            LastDir = South,
        
            Count,
        }

        private Vector2Int GetDirection() => Value switch
        {
            CardinalDirection.West  =>   _westVec,
            CardinalDirection.East  =>   _eastVec,
            CardinalDirection.North =>   _northVec,
            CardinalDirection.South =>   _southVec,
            _                       =>   _noDirVec,
        };

        public static Direction GetRandomDirection() => new (Random.Range(FirstDir, Count));
        public static Direction GetRandomPositiveDirection() => new (Random.Range(East, LastDir));

        public static implicit operator int(Direction direction) => (int)direction.Value;

        public static implicit operator Direction(int direction) => new (direction);

        public static implicit operator CardinalDirection(Direction direction) => direction.Value;

        public static implicit operator Direction(CardinalDirection direction) => new (direction);

        public static implicit operator Vector2Int(Direction direction) => direction.Vector;

        public static implicit operator Direction(Vector2Int direction) => new (direction);

    }


}
