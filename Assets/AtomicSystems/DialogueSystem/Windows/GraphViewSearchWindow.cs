
namespace DUJAL.Systems.Dialogue 
{
    using DUJAL.Systems.Dialogue.Constants;
    using System.Collections.Generic;
    using UnityEditor.Experimental.GraphView;
    using UnityEngine;
    public class GraphViewSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private DialogueSystemGraphView _graphView;
        private Texture2D _indentation;
        public void Initialize(DialogueSystemGraphView graphView) 
        {
            _graphView = graphView;

            _indentation = new Texture2D(1, 1);
            _indentation.SetPixel(0, 0, Color.clear);
            _indentation.Apply();
        }
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTreeEntries = new List<SearchTreeEntry>()
            {
                new SearchTreeGroupEntry(new GUIContent(DialogueConstants.SearchWindowCreateElementText)),
                new SearchTreeGroupEntry(new GUIContent(DialogueConstants.SearchWindowDialogueNodeText), 1),
                new SearchTreeEntry(new GUIContent(DialogueConstants.SearchWindowSinglechoiceText, _indentation))
                {
                    level = 2,
                    userData = DialogueType.SingleChoice
                },
                new SearchTreeEntry(new GUIContent(DialogueConstants.SearchWindowMultipleChoiceText, _indentation))
                {
                    level = 2,
                    userData = DialogueType.MultipleChoice
                },
                new SearchTreeGroupEntry(new GUIContent(DialogueConstants.SearchWindowDialogueGroupText), 1),
                new SearchTreeEntry(new GUIContent(DialogueConstants.SearchWindowDialogueSingleGroupText, _indentation))
                {
                    level = 2,
                    userData = new CustomGroup(DialogueConstants.AddGroupContextMenuText, Vector2.zero)
                }
            };
            return searchTreeEntries;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            Vector2 localMousePosition = _graphView.GetLocalMousePosition(context.screenMousePosition, true);
            switch (SearchTreeEntry.userData) 
            {
                case DialogueType.SingleChoice:
                    _graphView.AddElement((SingleChoiceNode)_graphView.CreateNode(DialogueConstants.BaseNodeDefaultNodeName, 
                        DialogueType.SingleChoice, localMousePosition));
                    return true;

                case DialogueType.MultipleChoice:
                    _graphView.AddElement((MultipleChoiceNode)_graphView.CreateNode(DialogueConstants.BaseNodeDefaultNodeName, 
                        DialogueType.MultipleChoice, localMousePosition));
                    return true;

                case CustomGroup _:
                    _graphView.CreateGroup(DialogueConstants.NewGroupDefaultName, localMousePosition);
                    return true;

                default:
                    return false;
            }
        }
    }
}
