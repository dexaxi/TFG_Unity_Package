namespace DUJAL.Systems.Dungeons
{
    using System.Collections.Generic;
    using UnityEngine;

    public class Graph
    {
        private List<List<int>> _adyacencies;
        public List<List<int>> GetAdjacencyList() => _adyacencies;
        public int Size { get; private set; }

        public Graph(int size)
        {
            if (size == 0)
            {
                Debug.LogError("Graph size must be greater than zero");
            }

            _adyacencies = new List<List<int>>();

            Size = size;

            for (uint i = 0; i < size; ++i)
            {
                _adyacencies.Add(new List<int>());
            }
        }

        public void AddEdge(int source, int destination)
        {
            if (source >= Size)
            {
                Debug.LogError("Argument out of Range " + (nameof(source)));
            }
            if (destination >= Size)
            {
                Debug.LogError("Argument out of Range " + (nameof(destination)));
            }
            _adyacencies[source].Add(destination);
        }

        public void AddEdgeBothWays(int source, int destination)
        {
            if (source >= Size)
            {
                Debug.LogError("Argument out of Range " + (nameof(source)));
            }
            if (destination >= Size)
            {
                Debug.LogError("Argument out of Range " + (nameof(destination)));
            }
            _adyacencies[source].Add(destination);
            _adyacencies[destination].Add(source);
        }

        public int[,] GetAdjacencyMatrix()
        {
            int[,] adjacencyMatrix = new int[Size, Size];
            for (int i = 0; i < Size; ++i)
            {
                foreach (int j in _adyacencies[i])
                {
                    ++adjacencyMatrix[i, j];
                }
            }
            return adjacencyMatrix;
        }

        public List<int> BFS(int source) => BFS(source, false);

        public List<int> LeveledBFS(int source) => BFS(source, true);

        private List<int> BFS(int source, bool leveled = false)
        {
            if (source >= Size)
            {
                Debug.LogError("Argument out of Range " + (nameof(source)));
                return new();
            }

            int node = 0;

            List<int> nodesToVisit = new()
        {
            source
        };

            if (leveled)
            {
                nodesToVisit.Add(-1);
            }

            bool[] visited = new bool[Size];

            visited[source] = true;

            while (node < nodesToVisit.Count)
            {
                int nodeVal = nodesToVisit[node];
                if (nodeVal == -1)
                {
                    ++node;
                    if (node != nodesToVisit.Count)
                    {
                        nodesToVisit.Add(-1);
                    }
                }
                else
                {
                    foreach (int adyacency in _adyacencies[nodeVal])
                    {
                        if (!visited[adyacency])
                        {
                            visited[adyacency] = true;
                            nodesToVisit.Add(adyacency);
                        }
                    }
                    ++node;
                }
            }
            return nodesToVisit;
        }

        public List<int> DFS(int source)
        {
            List<int> nodeList = new();
            Stack<int> remainingNodes = new();

            remainingNodes.Push(source);

            bool[] visited = new bool[Size];
            visited[source] = true;

            while (remainingNodes.Count > 0)
            {
                int node = remainingNodes.Peek();
                remainingNodes.Pop();
                if (!visited[node])
                {
                    nodeList.Add(node);
                    visited[node] = true;
                }
                foreach (int adyacency in _adyacencies[node])
                {
                    if (!visited[adyacency])
                    {
                        remainingNodes.Push(adyacency);
                    }
                }
            }
            return nodeList;
        }

        public int[] ConnectedComponents()
        {
            int[] connectedComponents = new int[Size];
            for (int i = 0; i < Size; ++i)
            {
                connectedComponents[i] = -1;
            }

            int nodeVal = 0;

            Queue<int> componentQueue = new();

            for (int node = 0; node < Size; ++node)
            {
                if (connectedComponents[node] < 0)
                {
                    componentQueue.Enqueue(node);
                    connectedComponents[node] = nodeVal;
                    while (componentQueue.Count > 0)
                    {
                        foreach (int adyacency in _adyacencies[componentQueue.Dequeue()])
                        {
                            if (connectedComponents[adyacency] < 0)
                            {
                                connectedComponents[adyacency] = nodeVal;
                                componentQueue.Enqueue(adyacency);
                            }
                        }
                    }
                    ++nodeVal;
                }
            }
            return connectedComponents;
        }

        public List<int> GetLeaves()
        {
            List<int> leaves = new List<int>();
            for (int index = 0; index < Size; ++index)
            {
                if (_adyacencies[index].Count == 1)
                {
                    leaves.Add(index);
                }
            }
            return leaves;
        }

        public void InvertGraph()
        {
            List<List<int>> invertedGraph = new();

            for (int i = 0; i < Size; ++i)
            {
                invertedGraph[i] = new List<int>();
            }

            for (int i = 0; i < Size; ++i)
            {
                foreach (int adyacency in _adyacencies[i])
                {
                    invertedGraph[adyacency].Add(i);
                }
            }

            _adyacencies = invertedGraph;
        }

        public bool AreNeighbours(int source, int destination)
        {
            if (source >= Size)
            {
                return false;
            }
            if (destination >= Size)
            {
                return false;
            }

            return _adyacencies[source].Contains(destination);
        }

        public bool IsReachable(int source, int destination)
        {
            if (source >= Size)
            {
                return false;
            }
            if (destination >= Size)
            {
                return false;
            }

            Queue<int> nodeQueue = new();

            nodeQueue.Enqueue(source);

            bool[] visited = new bool[Size];
            visited[source] = true;

            bool isReachable = false;

            while (nodeQueue.Count > 0 && !isReachable)
            {
                foreach (int adyacency in _adyacencies[nodeQueue.Dequeue()])
                {
                    if (!visited[adyacency])
                    {
                        visited[adyacency] = true;
                        nodeQueue.Enqueue(adyacency);
                        if (adyacency == destination)
                        {
                            isReachable = true;
                        }
                    }
                }
            }

            return isReachable;
        }
    }
}