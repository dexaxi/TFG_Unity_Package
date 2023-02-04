using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;

namespace DUJAL.Systems.Dialogue
{
    public class DialogueSystemEditorWindow : EditorWindow
    {
        [MenuItem("DUJAL/Dialogue Graph")]
        public static void Open()
        {
            GetWindow<DialogueSystemEditorWindow>("Dialogue Graph");
        }

        private void OnEnable()
        {
            AddGraphView();
            AddStyles();
        }

        private void AddGraphView()
        {
            DialogueSystemGraphView gView = new DialogueSystemGraphView();
            gView.StretchToParentSize();
            rootVisualElement.Add(gView);
        }

        private void AddStyles()
        {
            StyleSheet style = (StyleSheet)EditorGUIUtility.Load("Assets/AtomicSystems/DialogueSystem/EditorDefaultResources/Variables.uss");

            rootVisualElement.styleSheets.Add(style);
        }

    }
}