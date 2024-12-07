namespace DUJAL.DungeonCreator.Types 
{
    using System;
    using UnityEngine;

    class Edge : IComparable<Edge>
    {
        public Vector2Int origin;
        public Vector2Int end;
        private readonly int weight;

        internal Edge(Vector2Int originVec, Vector2Int endVec, int edgeWeight)
        {
            origin = originVec;
            end = endVec;
            weight = edgeWeight;
        }

        public int CompareTo(Edge a) => weight - a.weight;
    }
}