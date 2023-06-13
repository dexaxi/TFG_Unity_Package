

namespace DUJAL.Systems.Dialogue.Utils 
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using DUJAL.Systems.Dialogue.Constants;
    using System.Linq;
    using DUJAL.Systems.Utils;
    using UnityEditor.Experimental.GraphView;
    public class DialogueSystemIO
    {
        private static DialogueSystemGraphView _graphView;
        private static string _graphFilename;
        private static string _containerFolderPath;

        private static List<BaseNode> _nodes;
        private static List<CustomGroup> _groups;

        private static Dictionary<string, GroupScriptableObject> _createdGroups;
        private static Dictionary<string, DialogueScriptableObject> _createdDialogues;

        private static Dictionary<string, CustomGroup> _loadedGroups;
        private static Dictionary<string, BaseNode> _loadedNodes;
        public static void Initialize(DialogueSystemGraphView graphView, string name) 
        {
            _graphView = graphView;

            _graphFilename = name;
            _containerFolderPath = $"{DialogueConstants.AssetsFolderName}/{DialogueConstants.DialogueSystemDataContainerFolderName}/{_graphFilename}";

            _nodes = new List<BaseNode>();
            _groups = new List<CustomGroup>();

            _createdGroups = new Dictionary<string, GroupScriptableObject>();
            _createdDialogues = new Dictionary<string, DialogueScriptableObject>();

            _loadedGroups = new Dictionary<string, CustomGroup>();
            _loadedNodes = new Dictionary<string, BaseNode>();
        }

        public static void Save() 
        {
            CreateStaticFolders();
            GetElementsFromGraphView();
            GraphSaveDataScriptableObject graphData = CreateAsset<GraphSaveDataScriptableObject>($"{DialogueConstants.DialogueEditorGraphsPath}/" +
                $"{DialogueConstants.DialogueEditorGraphsFolder}", $"{_graphFilename}Graph");

            graphData.Initialize(_graphFilename);

            DialogueContainerScriptableObject dialogueContainerData = CreateAsset<DialogueContainerScriptableObject>(_containerFolderPath, _graphFilename);
            dialogueContainerData.Initialize(_graphFilename);

            SaveGroups(graphData, dialogueContainerData);
            SaveNodes(graphData, dialogueContainerData);

            SaveAsset(graphData);
            SaveAsset(dialogueContainerData);

            DialogueScriptableObject _currentPlayedText = CreateAsset<DialogueScriptableObject>(DialogueConstants.DialogueEditorGraphsPath, "Playable Dialogue");
        }

        public static void Load() 
        {
            GraphSaveDataScriptableObject graphData = LoadAsset<GraphSaveDataScriptableObject>($"{DialogueConstants.DialogueEditorGraphsPath}/" +
                $"{DialogueConstants.DialogueEditorGraphsFolder}", _graphFilename);

            if (graphData == null) 
            {
                EditorUtility.DisplayDialog(
                    DialogueConstants.LoadInvalidFilenamePopupTitle,
                    $"{DialogueConstants.LoadInvalidFilenamePopupText}\n\n" + 
                    $"{DialogueConstants.DialogueEditorGraphsPath}/{DialogueConstants.DialogueEditorGraphsFolder}/{_graphFilename}\n\n" +
                    DialogueConstants.LoadInvalidFilenamePopupText2,
                    DialogueConstants.LoadInvalidFilenamePopupPrompt
                    );
                return;
            }
            DialogueSystemEditorWindow.UpdateFileName(graphData.FileName);

            LoadGroups(graphData.Groups);
            LoadNodes(graphData.Nodes);
            LoadNodeConnections();
        }

        private static void LoadGroups(List<GroupSaveData> groups) 
        {
            foreach (GroupSaveData groupData in groups) 
            {
                CustomGroup group = _graphView.CreateGroup(groupData.Name, groupData.Position);
                group.ID = groupData.ID;
                _loadedGroups.Add(group.ID, group);
            }
        }

        private static void LoadNodes(List<NodeSaveData> nodes) 
        {
            foreach (NodeSaveData nodeData in nodes) 
            {
                List<ChoiceSaveData> choiceDataList = CloneChoiceDataList(nodeData.Choices);

                BaseNode node = _graphView.CreateNode(nodeData.Name, nodeData.DialogueType, nodeData.Position, false);
                node.ID = nodeData.ID;
                node.Choices = choiceDataList;
                node.Text = nodeData.Text;
                node.ImageField.value = nodeData.SpeakerSprite;
                node.VoiceLineField.value = nodeData.VoiceLine;
                node.DialogueColorField.value = nodeData.BackgroundColor;
                node.Draw();

                _graphView.AddElement(node);
                _loadedNodes.Add(node.ID, node);

                if (string.IsNullOrEmpty(nodeData.GroupID)) 
                {
                    continue;
                }

                CustomGroup group = _loadedGroups[nodeData.GroupID];
                node.Group = group;
                group.AddElement(node);
            }    
        }

        private static void LoadNodeConnections() 
        {
            foreach (KeyValuePair<string, BaseNode> loadedNode in _loadedNodes) 
            {
                foreach (Port choicePort in loadedNode.Value.outputContainer.Children()) 
                {
                    ChoiceSaveData choiceData = (ChoiceSaveData)choicePort.userData;
                    if (string.IsNullOrEmpty(choiceData.NodeID)) 
                    {
                        continue;
                    }
                    BaseNode connectedNode = _loadedNodes[choiceData.NodeID];
                    Port connectedNodeInputPort = (Port)connectedNode.inputContainer.Children().First();
                    Edge newEdge = choicePort.ConnectTo(connectedNodeInputPort);
                    _graphView.AddElement(newEdge);
                    loadedNode.Value.RefreshPorts();
                }
            }
        }

        private static void CreateStaticFolders()
        {
            CreateFolder(DialogueConstants.AssetsFolderName, DialogueConstants.DialogueSystemDataContainerFolderName);

            CreateFolder(DialogueConstants.DialogueEditorGraphsPath, DialogueConstants.DialogueEditorGraphsFolder);


            CreateFolder($"{DialogueConstants.AssetsFolderName}/{DialogueConstants.DialogueSystemDataContainerFolderName}", _graphFilename);

            CreateFolder(_containerFolderPath, DialogueConstants.GlobalContainerFolderName);
            CreateFolder(_containerFolderPath, DialogueConstants.GroupsContainerFolderName);
        }

        private static void CreateFolder(string path, string name)
        {
            if (AssetDatabase.IsValidFolder($"{path}/{name}")) 
            {
                return;    
            }
            AssetDatabase.CreateFolder(path, name);
        }
        private static void RemoveFolder(string fullPath)
        {
            FileUtil.DeleteFileOrDirectory($"{fullPath}.meta");
            FileUtil.DeleteFileOrDirectory($"{fullPath}/");
        }

        private static void GetElementsFromGraphView()
        {
            _graphView.graphElements.ForEach(graphElement =>
            {
                if (graphElement is BaseNode node)
                {
                    _nodes.Add(node);
                    return;
                }

                if (graphElement is CustomGroup group)
                {
                    _groups.Add(group);
                    return;
                }
            });
        }
        public static T CreateAsset<T> (string path, string name) where T : ScriptableObject
        {
            string fullPath = $"{path}/{name}.asset";
            T asset = AssetDatabase.LoadAssetAtPath<T>(fullPath);
            if (asset == null) 
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, fullPath);
            }
            return asset;
        }

        public static T LoadAsset<T>(string path, string name) where T : ScriptableObject
        {
            string fullPath = $"{path}/{name}.asset";
            return AssetDatabase.LoadAssetAtPath<T>(fullPath);
        }

        private static void SaveGroups(GraphSaveDataScriptableObject graphData, DialogueContainerScriptableObject dialogueContainerData)
        {
            List<string> groupTitles = new List<string>();
            foreach (CustomGroup group in _groups) 
            {
                SaveGroupToGraphData(group, graphData);
                SaveGroupToSO(group, dialogueContainerData);
                groupTitles.Add(group.title);
            }

            UpdateOldGroups(groupTitles, graphData);
        }

        private static void SaveGroupToGraphData(CustomGroup group, GraphSaveDataScriptableObject graphData)
        {
            GroupSaveData groupData = new GroupSaveData()
            {
                ID = group.ID,
                Name = group.title,
                Position = group.GetPosition().position
            };

            graphData.Groups.Add(groupData);
        }
        private static void SaveGroupToSO(CustomGroup group, DialogueContainerScriptableObject dialogueContainerData)
        {
            string groupTitle = group.title;
            CreateFolder($"{_containerFolderPath}/{DialogueConstants.GroupsContainerFolderName}", groupTitle);

            GroupScriptableObject groupSO = CreateAsset<GroupScriptableObject>($"{_containerFolderPath}/{DialogueConstants.GroupsContainerFolderName}/{groupTitle}", groupTitle);
            groupSO.Initialize(groupTitle);

            _createdGroups.Add(group.ID, groupSO);

            dialogueContainerData.DialogueGroups.Add(groupSO, new List<DialogueScriptableObject>());

            SaveAsset(groupSO);
        }
         
        private static void UpdateOldGroups(List<string> currentGroupTitles, GraphSaveDataScriptableObject graphData)
        {
            if (graphData.PreviousGroupTitles == null || graphData.PreviousGroupTitles.Count == 0) 
            {
                graphData.PreviousGroupTitles = new List<string>(currentGroupTitles);
                return;
            }

            List<string> removableTitles = graphData.PreviousGroupTitles.Except(currentGroupTitles).ToList();

            foreach (string removableGroupTitle in removableTitles) 
            {
                RemoveFolder($"{_containerFolderPath}/{DialogueConstants.GroupsContainerFolderName}/{removableGroupTitle}");
            }

            graphData.PreviousGroupTitles = new List<string>(currentGroupTitles);
        }

        private static void SaveNodes(GraphSaveDataScriptableObject graphData, DialogueContainerScriptableObject dialogueContainerData)
        {
            SerializableDictionary<string, List<string>> groupedNodeNames = new SerializableDictionary<string, List<string>>();
            List<string> ungroupedNodeNames = new List<string>();

            foreach (BaseNode node in _nodes) 
            {
                SaveNodeToGraphData(node, graphData);
                SaveNodeToSO(node, dialogueContainerData);
                if (node.Group != null)
                {
                    groupedNodeNames.AddItem(node.Group.title, node.DialogueName);
                }
                else 
                {
                    ungroupedNodeNames.Add(node.DialogueName);
                }
            }

            UpdateChoiceConnections();
            UpdateOldGroupedNodes(groupedNodeNames, graphData);
            UpdateOldUngroupedNodes(ungroupedNodeNames, graphData);
        }

        private static void SaveNodeToGraphData(BaseNode node, GraphSaveDataScriptableObject graphData)
        {
            List<ChoiceSaveData> choiceDataList = CloneChoiceDataList(node.Choices);

            NodeSaveData nodeData = new NodeSaveData()
            {
                ID = node.ID,
                Name = node.DialogueName,
                Choices = choiceDataList,
                Text = node.Text,
                GroupID = node.Group?.ID,
                DialogueType = node.DialogueType,
                Position = node.GetPosition().position,
                BackgroundColor = node.DialogueColorField.value,
                VoiceLine = (AudioClip)node.VoiceLineField.value,
                SpeakerSprite = (Sprite)node.ImageField.value,
            };

            graphData.Nodes.Add(nodeData);
        }

        private static List<ChoiceSaveData> CloneChoiceDataList(List<ChoiceSaveData> choices)
        {
            List<ChoiceSaveData> choiceDataList = new List<ChoiceSaveData>();

            foreach (ChoiceSaveData choice in choices)
            {
                ChoiceSaveData c = new ChoiceSaveData()
                {
                    Text = choice.Text,
                    NodeID = choice.NodeID
                };

                choiceDataList.Add(c);
            }

            return choiceDataList;
        }

        private static void SaveNodeToSO(BaseNode node, DialogueContainerScriptableObject dialogueContainerData)
        {
            DialogueScriptableObject dialogueSO;
            if (node.Group != null)
            {
                dialogueSO = CreateAsset<DialogueScriptableObject>($"{_containerFolderPath}/{DialogueConstants.GroupsContainerFolderName}/{node.Group.title}", node.DialogueName);

                dialogueContainerData.DialogueGroups.AddItem(_createdGroups[node.Group.ID], dialogueSO);
            }
            else 
            {
                dialogueSO = CreateAsset<DialogueScriptableObject>($"{_containerFolderPath}/{DialogueConstants.GlobalContainerFolderName}", node.DialogueName);

                dialogueContainerData.UngroupedDialogues.Add(dialogueSO);
            }

            dialogueSO.Initialize(
                node.DialogueName,
                node.Text,
                node.Choices.ConvertChoiceLists(),
                node.DialogueType,
                node.IsStartingNode(), 
                (AudioClip) node.VoiceLineField.value,
                (Sprite) node.ImageField.value,
                node.DialogueColorField.value);
            _createdDialogues.Add(node.ID, dialogueSO);
            SaveAsset(dialogueSO);
        }

        private static void UpdateChoiceConnections()
        {
            foreach (BaseNode node in _nodes) 
            {
                DialogueScriptableObject dialogueSO = _createdDialogues[node.ID];

                for (int i = 0; i < node.Choices.Count; i++) 
                {
                    ChoiceSaveData choiceData = node.Choices[i];
                    if (string.IsNullOrEmpty(choiceData.NodeID)) 
                    {
                        continue;
                    }
                    dialogueSO.Choices[i].NextDialogue = _createdDialogues[choiceData.NodeID];
                    SaveAsset(dialogueSO);
                }
            }
        }

        private static void UpdateOldUngroupedNodes(List<string> currentNodeNames, GraphSaveDataScriptableObject graphData)
        {
            if (graphData.UngroupedNodesPreviousNames == null || graphData.UngroupedNodesPreviousNames.Count == 0) 
            {
                graphData.UngroupedNodesPreviousNames = new List<string>(currentNodeNames);
                return;
            }

            List<string> removableNodeNames = graphData.UngroupedNodesPreviousNames.Except(currentNodeNames).ToList();

            foreach (string removableNode in removableNodeNames) 
            {
                RemoveAsset($"{_containerFolderPath}/{DialogueConstants.GlobalContainerFolderName}/", removableNode);
            }

            graphData.UngroupedNodesPreviousNames = new List<string>(currentNodeNames);
        }

        private static void UpdateOldGroupedNodes(SerializableDictionary<string, List<string>> currentNodeNames, GraphSaveDataScriptableObject graphData)
        {
            if (graphData.GroupedNodesPreviousNames == null || graphData.GroupedNodesPreviousNames.Count == 0)
            {
                currentNodeNames.CopySerializableDictionary(graphData.GroupedNodesPreviousNames);
                return;
            }

            foreach (KeyValuePair<string, List<string>> oldGroupedNode in graphData.GroupedNodesPreviousNames) 
            {
                List<string> removableNodeNames = new List<string>();
                if (currentNodeNames.ContainsKey(oldGroupedNode.Key)) 
                {
                    removableNodeNames = oldGroupedNode.Value.Except(currentNodeNames[oldGroupedNode.Key]).ToList();

                    foreach (string removableNode in removableNodeNames) 
                    {
                        RemoveAsset($"{_containerFolderPath}/{DialogueConstants.GroupsContainerFolderName}/{oldGroupedNode.Key}", removableNode);
                    }
                }
            }
            currentNodeNames.CopySerializableDictionary(graphData.GroupedNodesPreviousNames);
        }

        public static void RemoveAsset(string path, string assetName)
        {
            string fullAssetPath = $"{path}/{assetName}.asset";
            AssetDatabase.DeleteAsset(fullAssetPath);  
        }

        public static void SaveAsset(UnityEngine.Object asset) 
        {
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

    }
}
