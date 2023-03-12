using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DUJAL.Systems.Dialogue.Utils
{
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
            foreach(string uss in styleSheetNames) 
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
    }
}
