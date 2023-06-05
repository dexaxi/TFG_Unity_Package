
namespace DUJAL.Systems.Dialogue.Utils
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.Experimental.GraphView;
    using UnityEngine.UIElements;
    using DUJAL.Systems.Utils;
    public static class DialogueSystemUtils
    {
        //Node Utils
        public static TextField CreateTextField(string value = null, string label = null, EventCallback<ChangeEvent<string>> onValueChanged = null)
        {
            TextField tf = new TextField()
            {
                value = value,
                label = label
            };

            if (onValueChanged != null)
            {
                tf.RegisterValueChangedCallback(onValueChanged);
            }

            return tf;
        }

        public static TextField CreateTextArea(string value = null, string label = null, EventCallback<ChangeEvent<string>> onValueChanged = null)
        {
            TextField textArea = CreateTextField(value, label, onValueChanged);
            textArea.multiline = true;
            return textArea;
        }

        public static Foldout CreateFoldout(string title, bool collapsed = false)
        {
            Foldout foldout = new Foldout()
            {
                text = title,
                value = !collapsed
            };

            return foldout;
        }

        public static Button CreateButon(string text, Action onClick = null)
        {
            Button button = new Button(onClick)
            {
                text = text
            };
            return button;
        }

        public static Port CreatePort(this BaseNode node, string portName = "", Orientation orientation = Orientation.Horizontal, Direction direction = Direction.Output, Port.Capacity capacity = Port.Capacity.Single)
        {
            Port port = node.InstantiatePort(orientation, direction, capacity, typeof(bool));
            port.portName = portName;
            return port;
        }

        //Style Utils
        public static VisualElement AddStyleSheets(this VisualElement element, params string[] styleSheetNames)
        {
            foreach (string uss in styleSheetNames)
            {
                StyleSheet sheet = (StyleSheet)EditorGUIUtility.Load(uss);
                element.styleSheets.Add(sheet);
            }
            return element;
        }

        public static VisualElement AddClasses(this VisualElement element, params string[] classNames)
        {
            foreach (string c in classNames)
            {
                element.AddToClassList(c);
            }

            return element;
        }

        //SerializableDictionaryUtil
        public static void AddItem<K, V>(this SerializableDictionary<K, List<V>> dictionary, K key, V value)
        {
            if (dictionary.ContainsKey(key)) 
            {
                dictionary[key].Add(value);
                return;
            }

            dictionary.Add(key, new List<V>() { value });
        }

        //Saving/Loading
        public static List<ChoiceData> ConvertChoiceLists(this List<ChoiceSaveData> choiceSaveData) 
        {
            List<ChoiceData> returnableChoices = new List<ChoiceData>();

            foreach (ChoiceSaveData choiceSave in choiceSaveData) {
                ChoiceData choiceData = new ChoiceData()
                {
                    Text = choiceSave.Text
                };
                returnableChoices.Add(choiceData);
            }
            return returnableChoices;
        }

        //Custom Inspector
        public static void DrawDisabledFields(Action a)
        {
            EditorGUI.BeginDisabledGroup(true);
            a.Invoke();
            EditorGUI.EndDisabledGroup();
        }
        public static void DrawHeader(string label) 
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        }

        public static void DrawPropertyField(this SerializedProperty property) 
        {
            EditorGUILayout.PropertyField(property);
        }

        public static int DrawPopup(string label, SerializedProperty index, string[] options)
        {
            return EditorGUILayout.Popup(label, index.intValue, options);
        }
        public static int DrawPopup(string label, int index, string [] options) 
        {
            return EditorGUILayout.Popup(label, index, options);
        } 

        public static void DrawSpace(int space = 5) 
        {
            EditorGUILayout.Space(space);
        }

        public static void DrawHelp(string title, MessageType type = MessageType.Info, bool wide = true) 
        {
            EditorGUILayout.HelpBox(title, type, wide);
        }

        //Update property Index in custom editor 
        public static void UpdateIndexOnNameListChange(this SerializedProperty selectedIndexProperty, List<string> options, int previousIndex, string oldName, bool isOldNull)
        {
            if (isOldNull)
            {
                selectedIndexProperty.intValue = 0;
                return;
            }
            bool prevIndexOOB = previousIndex > options.Count - 1;
            bool notCurrentName = prevIndexOOB || oldName != options[previousIndex];
            if (notCurrentName)
            {
                if (options.Contains(oldName))
                {
                    selectedIndexProperty.intValue = options.IndexOf(oldName);
                }
                else
                {
                    selectedIndexProperty.intValue = 0;
                }
            }
        }
    }
}
