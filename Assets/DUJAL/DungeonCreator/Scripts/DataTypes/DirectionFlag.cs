namespace DUJAL.DungeonCreator.Types
{
    using System.Collections.Generic;
    using System;

    [Serializable]
    public struct DirectionFlag
    {
        public bool West;
        public bool East;
        public bool North;
        public bool South;

        public DirectionFlag(List<Direction> directions)
        {
            West = false;
            East = false;
            North = false;
            South = false;

            foreach (Direction dir in directions)
            {
                switch (dir.Value)
                {
                    case Direction.CardinalDirection.West:
                        West = true;
                        break;
                    case Direction.CardinalDirection.East:
                        East = true;
                        break;
                    case Direction.CardinalDirection.North:
                        North = true;
                        break;
                    case Direction.CardinalDirection.South:
                        South = true;
                        break;
                }
            }

            West  =   West    || false;
            East  =   East    || false;
            North =   North   || false;
            South =   South   || false;
        }

        public DirectionFlag(bool west, bool east, bool north, bool south)
        {
            West = west;
            East = east;
            North = north;
            South = south;
        }

        public readonly bool IsDirectionEnabled(Direction dir) => dir.Value switch
        {
            Direction.CardinalDirection.West  => West,
            Direction.CardinalDirection.East  => East,
            Direction.CardinalDirection.North => North,
            Direction.CardinalDirection.South => South,
            _                                 => false,
        };

        public readonly bool IsValid() 
        {
            return this != new DirectionFlag (false, false, false, false);
        }

        public readonly override string ToString()
        {
            return "Directions: { W: " + West + ", E: " + East + ", N: " + North + ", S: " + South + " }";
        }

        private static bool AreEquals(DirectionFlag direction, DirectionFlag other)
        {
            return direction.West == other.West && direction.East == other.East && direction.North == other.North && direction.South == other.South;
        }

        public readonly override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            return this == (DirectionFlag) obj;
        }

        public override readonly int GetHashCode() => HashCode.Combine(West, East, North, South);

        public static bool operator ==(DirectionFlag direction, DirectionFlag other) => AreEquals(direction, other);
        public static bool operator !=(DirectionFlag direction, DirectionFlag other) => !AreEquals(direction, other);
    }
}