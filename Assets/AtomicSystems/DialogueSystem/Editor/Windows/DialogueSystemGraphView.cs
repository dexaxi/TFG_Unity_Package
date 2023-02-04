using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace DUJAL.Systems.Dialogue
{
    using System.Reflection;
    using UnityEngine;
    public class DialogueSystemGraphView : GraphView
    {
        public DialogueSystemGraphView() 
        {
            AddManipulators();
            AddGridBackground();
                        
            AddStyles();
        }


        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(CreateContextMenu("Add Single Choice Node", DialogueType.SingleChoice));
            this.AddManipulator(CreateContextMenu("Add Multiple Choice Node", DialogueType.MultipleChoice));
        }

        private IManipulator CreateContextMenu(string actionTitle, DialogueType type) 
        {
            ContextualMenuManipulator contexMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction(actionTitle, actionEvent => AddElement(CreateNode(type, actionEvent.eventInfo.localMousePosition)))
                );

            return contexMenuManipulator;
        }

        private Node CreateNode(DialogueType type,Vector2 pos)
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
            node.Initialize(pos);
            node.Draw();
            return node;
        }

        private void AddGridBackground()
        {
            GridBackground gBackground = new GridBackground();
            gBackground.StretchToParentSize();
            Insert(0, gBackground);
        }

        private void AddStyles()
        {
            StyleSheet style = (StyleSheet)EditorGUIUtility.Load("Assets/AtomicSystems/DialogueSystem/EditorDefaultResources/GraphViewStyle.uss");

            styleSheets.Add(style);
        }
    }
}
   