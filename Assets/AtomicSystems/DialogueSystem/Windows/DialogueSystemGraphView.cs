using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using DUJAL.Systems.Dialogue.Utils;
using DUJAL.Systems.Utils;

namespace DUJAL.Systems.Dialogue
{
    using DUJAL.Systems.Dialogue.Constants;
    using System.Collections.Generic;
    using UnityEngine;
    public class DialogueSystemGraphView : GraphView
    {
        private GraphViewSearchWindow _seachWindow;
        private DialogueSystemEditorWindow _editorWindow;

        private SerializableDictionary<string, DialogueSystemNodeError> _ungroupedNodes;
        private SerializableDictionary<string, DialogueSystemGroupError> _groupMap;
        private SerializableDictionary<CustomGroup, SerializableDictionary<string, DialogueSystemNodeError>> _groupedNodes;

        private int _hasError;
        public int HasError 
        {
            get => _hasError;
            set 
            {
                _hasError = value;
                if (_hasError == 0)
                {
                    _editorWindow.ToggleSaving(true);
                }
                if (_hasError == 1)
                {
                    _editorWindow.ToggleSaving(false);
                }
            }
        }
        public DialogueSystemGraphView(DialogueSystemEditorWindow editorWindow) 
        {
            _editorWindow = editorWindow;

            _ungroupedNodes = new SerializableDictionary<string, DialogueSystemNodeError>();
            _groupMap = new SerializableDictionary<string, DialogueSystemGroupError>();
            _groupedNodes = new SerializableDictionary<CustomGroup, SerializableDictionary<string, DialogueSystemNodeError>>();

            AddManipulators();
            AddSearchWindow(); 
            AddGridBackground();
            OnElementsDeleted();
            OnGroupElementsAdded();
            OnGroupElementsRemoved();
            OnGroupRenamed();
            AddStyles();
        }

        #region ContextMenus and Manipulators
        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(CreateContextMenu(DialogueConstants.AddSingleChoiceText, DialogueType.SingleChoice));
            this.AddManipulator(CreateContextMenu(DialogueConstants.AddMultiplechoiceText, DialogueType.MultipleChoice));
            this.AddManipulator(CreateGroupContextualMenu());
        }

        private void AddSearchWindow()
        {
            if (_seachWindow == null) 
            {
                _seachWindow = ScriptableObject.CreateInstance<GraphViewSearchWindow>();
                _seachWindow.Initialize(this);
            }
            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _seachWindow);
        }

        private IManipulator CreateContextMenu(string actionTitle, DialogueType type) 
        {
            ContextualMenuManipulator ctxMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction(actionTitle, actionEvent => AddElement(CreateNode(type, GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))))
                );

            return ctxMenuManipulator;
        }

        private IManipulator CreateGroupContextualMenu() 
        {
            ContextualMenuManipulator ctxMenuManipulator = new ContextualMenuManipulator(
                   menuEvent => menuEvent.menu.AppendAction(DialogueConstants.AddGroupContextMenuText, actionEvent => CreateGroup(DialogueConstants.NewGroupDefaultName, GetLocalMousePosition(actionEvent.eventInfo.localMousePosition)))
                );
            return ctxMenuManipulator;
        }

        public Vector2 GetLocalMousePosition(Vector2 mousePosition, bool isSearchWindow = false) 
        {
            Vector2 worldMousePosition = mousePosition;
            if (isSearchWindow) 
            {
                worldMousePosition -= _editorWindow.position.position;
            }
            Vector2 localMousePosition = contentViewContainer.WorldToLocal(worldMousePosition);
            return localMousePosition;
        }
        #endregion

        #region Style
        private void AddGridBackground()
        {
            GridBackground gBackground = new GridBackground();
            gBackground.StretchToParentSize();
            Insert(0, gBackground);
        }

        private void AddStyles()
        {
            this.AddStyleSheets(
                DialogueConstants.EditorDefaultResourcesPath + DialogueConstants.GraphViewStyleFilename,
                DialogueConstants.EditorDefaultResourcesPath + DialogueConstants.NodeStyleFilename
            );
        }
        #endregion

        #region Nodes and Groups
        public Node CreateNode(DialogueType type,Vector2 pos)
        {
            string assemblyName;

            switch (type) 
            {
                case DialogueType.MultipleChoice:
                    assemblyName = typeof(MultipleChoiceNode).AssemblyQualifiedName;
                    break;
                case DialogueType.SingleChoice:
                default:
                    assemblyName = typeof(SingleChoiceNode).AssemblyQualifiedName;
                    break;
            }

            Type nodeType = Type.GetType(assemblyName);
            BaseNode node = (BaseNode) Activator.CreateInstance(nodeType);
            node.Initialize(this, pos);
            node.Draw();

            AddUnGroupedNode(node);

            return node;
        }
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) 
        {
            List<Port> compatiblePorts = new List<Port>();
            ports.ForEach(port =>
            {
                if (startPort == port) 
                {
                    return;
                }
                if (startPort.node == port.node) 
                {
                    return;
                }
                if(startPort.direction == port.direction) 
                {
                    return;
                }
                compatiblePorts.Add(port);
            });
            return compatiblePorts;
        }

        public void AddUnGroupedNode(BaseNode node)
        {
            string dialogueName = node.DialogueName.ToLower();
            
            if (!_ungroupedNodes.ContainsKey(dialogueName))
            {
                DialogueSystemNodeError nodeError = new DialogueSystemNodeError();
                nodeError.BaseNodes.Add(node);
                _ungroupedNodes.Add(dialogueName, nodeError);
                return;
            }

            List<BaseNode> nodeList = _ungroupedNodes[dialogueName].BaseNodes;

            nodeList.Add(node);
            Color errorColor = _ungroupedNodes[dialogueName].ErrorData.Color;
            node.SetErrorStyle(errorColor);
            if (nodeList.Count == 2) 
            {
                ++HasError;
                nodeList[0].SetErrorStyle(errorColor);
            }
        }

        public void RemoveUngroupedNode(BaseNode node) 
        {
            string dialogueName = node.DialogueName.ToLower();
            List<BaseNode> nodeList = _ungroupedNodes[dialogueName].BaseNodes;
            nodeList.Remove(node);
            node.ResetStyleError();
            if (nodeList.Count == 1) 
            {
                --HasError;
                nodeList[0].ResetStyleError();
                return;
            }

            if (nodeList.Count == 0) 
            {
                _ungroupedNodes.Remove(dialogueName);
            }
        }

        public void AddGroupedNode(BaseNode node, CustomGroup group)
        {
            string dialogueName = node.DialogueName.ToLower();
            node.Group = group;
            if (!_groupedNodes.ContainsKey(group)) 
            {
                _groupedNodes.Add(group, new SerializableDictionary<string, DialogueSystemNodeError>());
            }

            if (!_groupedNodes[group].ContainsKey(dialogueName)) 
            {
                DialogueSystemNodeError nodeError = new DialogueSystemNodeError();
                nodeError.BaseNodes.Add(node);
                _groupedNodes[group].Add(dialogueName, nodeError);
                return;
            }

            List<BaseNode> nodeList = _groupedNodes[group][dialogueName].BaseNodes;

            nodeList.Add(node);
            Color errorColor = _groupedNodes[group][dialogueName].ErrorData.Color;
            node.SetErrorStyle(errorColor);

            if (nodeList.Count == 2) 
            {
                ++HasError;
                nodeList[0].SetErrorStyle(errorColor);
            }
        }
        public void RemoveGroupedNode(BaseNode node, CustomGroup group)
        {
            string dialogueName = node.DialogueName.ToLower();
            node.Group = null;
            List<BaseNode> nodeList = _groupedNodes[group][dialogueName].BaseNodes;
            nodeList.Remove(node);
            node.ResetStyleError();

            if (nodeList.Count == 1) 
            {
                --HasError;
                nodeList[0].ResetStyleError();
                return;
            }

            if (nodeList.Count == 0) 
            {
                _groupedNodes[group].Remove(dialogueName);
                if (_groupedNodes[group].Count == 0) 
                {
                    _groupedNodes.Remove(group);
                }
            }
        }

        public CustomGroup CreateGroup(string name, Vector2 localMousePos)
        {
            CustomGroup group = new CustomGroup(name, localMousePos);
            AddGroup(group);
            AddElement(group);
            foreach (GraphElement selected in selection) 
            {
                if (selected is BaseNode node) 
                {
                    group.AddElement(node);
                }
            }
            return group;
        }

        private void AddGroup(CustomGroup group)
        {
            string groupTitle = group.title.ToLower();
            if (!_groupMap.ContainsKey(groupTitle)) 
            {
                DialogueSystemGroupError groupError = new DialogueSystemGroupError();
                groupError.Groups.Add(group);
                _groupMap.Add(groupTitle, groupError);
                return;
            }

            List<CustomGroup> groupList = _groupMap[groupTitle].Groups;

            groupList.Add(group);
            Color errorColor = _groupMap[groupTitle].ErrorData.Color;
            group.SetErrorStyle(errorColor);
            if (groupList.Count == 2) 
            {
                ++HasError;
                groupList[0].SetErrorStyle(errorColor);
            }
        }

        private void RemoveGroup(CustomGroup group)
        {
            string previousGroupTitle = group.PreviousTitle.ToLower();
            List<CustomGroup> grouplist = _groupMap[previousGroupTitle].Groups;
            grouplist.Remove(group);
            group.ResetStyle();
            if (grouplist.Count == 1) 
            {
                --HasError;
                grouplist[0].ResetStyle();
                return;
            }
            if (grouplist.Count == 0) 
            {
                _groupMap.Remove(previousGroupTitle);
            }
        }

        private void OnGroupElementsAdded() 
        {
            elementsAddedToGroup = (group, elements) =>
            {
                foreach (GraphElement element in elements)
                {
                    if (!(element is BaseNode))
                    {
                        continue;
                    }
                    CustomGroup customGroup = (CustomGroup) group;
                    BaseNode node = (BaseNode) element;
                    RemoveUngroupedNode(node);
                    AddGroupedNode(node, customGroup);
                }
            };
        }

        private void OnGroupElementsRemoved()
        {
            elementsRemovedFromGroup = (group, elements) =>
            {
                foreach (GraphElement element in elements)
                {
                    if (!(element is BaseNode))
                    {
                        continue;
                    }
                    BaseNode node = (BaseNode)element;
                    CustomGroup customGroup = (CustomGroup) group;
                    RemoveGroupedNode(node, customGroup);
                    AddUnGroupedNode(node);
                }
            };
        }

        private void OnElementsDeleted()
        {
            deleteSelection = (operationName, askUser) =>
            {
                List<Edge> deleteableEdges = new List<Edge>();
                
                List<CustomGroup> deleteableGroups = new List<CustomGroup>();
                
                List<BaseNode> deleteableNodes = new List<BaseNode>();
                
                foreach (GraphElement element in selection) 
                {
                    if (element is BaseNode node) 
                    {
                        deleteableNodes.Add(node);
                        continue;
                    }
                    if (element is CustomGroup group) 
                    {
                        RemoveGroup(group);
                        deleteableGroups.Add(group);
                    }
                    if (element is Edge edge)
                    {
                        deleteableEdges.Add(edge);
                    }
                }

                foreach (CustomGroup group in deleteableGroups) 
                {
                    List<BaseNode> nodes = new List<BaseNode>();
                    foreach (GraphElement element in group.containedElements)
                    {
                        if (element is BaseNode node)
                        {
                            nodes.Add(node);
                        }
                    }
                    group.RemoveElements(nodes);
                    RemoveElement(group);
                }

                DeleteElements(deleteableEdges);

                foreach (BaseNode node in deleteableNodes) 
                {
                    if (node.Group != null) 
                    {
                        node.Group.RemoveElement(node);
                    }
                    RemoveUngroupedNode(node);
                    node.DisconnectAllPorts();
                    RemoveElement(node);
                }
            };
        }
        private void OnGroupRenamed() 
        {
            groupTitleChanged = (group, title) =>
            {
                CustomGroup customGroup = (CustomGroup) group;
                customGroup.title = title.RemoveWhitespaces().RemoveSpecialCharacters();
                RemoveGroup(customGroup);
                customGroup.PreviousTitle = customGroup.title;
                AddGroup(customGroup);
            };
        }
        #endregion

    }
}
   