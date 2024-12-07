namespace DUJAL.Systems.Dungeons.Utils
{
    using UnityEngine;
    using System.Collections.Generic;

    public static class ListUtils<T>
    {
        public static List<T> Shuffle(List<T> list)
        {
            List<T> objList = new();
            while (list.Count > 0)
            {
                int index = Random.Range(0, list.Count);
                objList.Add(list[index]);
                list.RemoveAt(index);
            }
            return objList;
        }

        public static T GetRandomElement(List<T> list) => list[Random.Range(0, list.Count)];

        public static T GetRandomElement(List<T> list, List<double> weights)
        {
            if (list == null || weights == null)
            {
                Debug.LogError("Null list or weights.");
                return default;
            }

            if (list.Count != weights.Count)
            {
                Debug.LogError("List of elements and list of weights must be the same size.");
                return default;
            }

            double totalWeight = 0.0;

            foreach (double weight in weights)
            {
                totalWeight += weight;
            }

            double maxWeight = RandomUtils.GetRandomDouble() * totalWeight;
            double totalCumulativeWeight = 0.0;

            int index = 0;
            while (totalCumulativeWeight < maxWeight)
            {
                totalCumulativeWeight += weights[index];
                ++index;
            }
            if (index != 0)
            {
                --index;
            }
            return list[index];
        }

        public static List<T> GetRandomElements(List<T> list, List<double> weights, uint count, bool regenRandomSeed = true)
        {
            if (count < list.Count && !regenRandomSeed)
            {
                Debug.LogError("Count is less than list size");
                return new List<T>();
            }

            HashSet<T> elementSet = new();
            List<T> elements = new();

            if (regenRandomSeed)
            {
                for (uint index = 0; index < count; ++index)
                {
                    elements.Add(GetRandomElement(list, weights));
                }
            }
            else
            {
                int num = 0;
                while (num < count)
                {
                    T element = GetRandomElement(list, weights);
                    if (!elementSet.Contains(element))
                    {
                        elementSet.Add(element);
                        elements.Add(element);
                        ++num;
                    }
                }
            }

            return elements;
        }
    }
}
