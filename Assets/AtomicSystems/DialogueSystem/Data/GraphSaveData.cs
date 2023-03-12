using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DUJAL.Systems.Utils;

namespace DUJAL.Systems.Dialogue
{
    public class GraphSaveData

    {
        [field: SerializeField] public string FileName { get; set; }
        [field: SerializeField] public List<GroupSaveData> Groups { get; set; }
        [field: SerializeField] public List<NodeSaveData> Nodes { get; set; }
        [field: SerializeField] public List<string> GroupPreviousNames { get; set; }
        [field: SerializeField] public List<string> UngroupedNodesPreviousNames { get; set; }
        [field: SerializeField] public SerializableDictionary<string, List<string>> GroupedNodesPreviousNames { get; set; }

        public void Initialize(string filename) 
        {
            FileName = filename;
            Groups= new List<GroupSaveData>();
            Nodes = new List<NodeSaveData>();
        }
    }
}
