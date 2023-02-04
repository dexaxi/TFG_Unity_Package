using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DUJAL.Systems.Dialogue
{
    public class BaseNode : Node
    {
        public string DialogueName { get; set; }
        public List<string> Choices { get; set; }
        public string Text { get; set; }
        public DialogueType DialogueType { get; set; }
        public ObjectField ImageField { get; set; }
        public ObjectField VoiceLineField { get; set; }

        public virtual void Initialize(Vector2 pos) 
        {
            DialogueName = "DialogueName";
            Choices = new List<string>();
            Text = "Dialogue Text";
            VoiceLineField = new ObjectField()
            {
                objectType = typeof(AudioClip),
                name = "Voice Line"
            };
            ImageField = new ObjectField()
            {
                objectType = typeof(Sprite),
                name = "Speaker Image",
            };

            SetPosition(new Rect(pos, Vector2.zero));
        }

        public virtual void Draw() 
        {
            //Title
            TextField nameTF = new TextField()
            {
                value = DialogueName
            };

            titleContainer.Insert(0, nameTF);

            //Input

            Port input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            input.portName = "Dialogue Connection";

            inputContainer.Add(input);

            //Ext. Container
            VisualElement customDC = new VisualElement();
            Foldout textF = new Foldout()
            {
                text = "Dialogue Text"
            };

            TextField textTF = new TextField()
            {
                value = Text
            };

            textF.Add(textTF);
            customDC.Add(textF);
            extensionContainer.Add(customDC);
            extensionContainer.Add(ImageField);
            extensionContainer.Add(VoiceLineField);
        }
    }   
}
