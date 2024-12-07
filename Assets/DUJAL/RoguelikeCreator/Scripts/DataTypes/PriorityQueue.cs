namespace DUJAL.Systems.Dungeons.Types 
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class PriorityQueue<T> where T : IComparable<T>
    {
        private List<T> _priorityQueue;
        public PriorityQueue() => _priorityQueue = new List<T>();
        public T Peek() => _priorityQueue[0];
        public int Count() => _priorityQueue.Count;
        public void Clear() => _priorityQueue.Clear();
        public void Release() => _priorityQueue = new List<T>();
        public bool IsEmpty() => _priorityQueue.Count == 0;

        public void Enqueue(T item)
        {
            _priorityQueue.Add(item);
            _priorityQueue.Sort(Compare);
        }

        private int Compare(T item1, T item2) 
        {
            return item2.CompareTo(item1);
        }

        public T Dequeue()
        {
            if (_priorityQueue.Count == 0) 
            {
                Debug.LogError("Queue is empty!");
                return default;
            }
            T returnObj = _priorityQueue[0];
            _priorityQueue.RemoveAt(0);
            return returnObj;
        }

        public override string ToString()
        {
            string returnStr = "";
            for (int i = 0; i < _priorityQueue.Count; i++)
            { 
                returnStr = returnStr + _priorityQueue[i].ToString() + " ";
            }
            return returnStr + "count = " + _priorityQueue.Count;
        }
    }
}