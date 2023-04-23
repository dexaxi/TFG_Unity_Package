namespace DUJAL.Systems.Dialogue
{
    using DUJAL.Systems.Dialogue.Constants;
    using DUJAL.Systems.Dialogue.Utils;
    using DUJAL.Systems.Utils;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor.Experimental.GraphView;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;
    public class BaseNode : Node
    {
        protected DialogueSystemGraphView _graphView;
        private StyleFloat _defaultBorderWidth;
        private StyleColor _defaultBorderColor;

        public string ID { get; set; }
        public string DialogueName { get; set; }
        public List<ChoiceSaveData> Choices { get; set; }
        public string Text { get; set; }
        public DialogueType DialogueType { get; set; }
        public CustomGroup Group { get; set; }
        public ObjectField ImageField { get; set; }
        public ObjectField VoiceLineField { get; set; }
        public CustomColorField DialogueColorField { get;  set; }

        public virtual void Initialize(string nodeName, DialogueSystemGraphView graphView, Vector2 pos) 
        {
            ID = Guid.NewGuid().ToString();
            DialogueName = nodeName;
            Choices = new List<ChoiceSaveData>(); 
            Text = DialogueConstants.BaseNodeQuoteTitle;
            VoiceLineField = new ObjectField()
            {
                objectType = typeof(AudioClip),
                name = DialogueConstants.BaseNodeVoiceLinetitle
            };
            ImageField = new ObjectField()
            {
                objectType = typeof(Sprite),
                name = DialogueConstants.BaseNodeImageField
            };
            DialogueColorField = new CustomColorField()
            {
                name = DialogueConstants.BaseNodeColorField
            };
            DialogueColorField.Changed += UpdateStyle;
            DialogueColorField.value = DialogueConstants.DefaultNodeBackgroundColor;

             _graphView = graphView;

            SetPosition(new Rect(pos, Vector2.zero));

            mainContainer.AddToClassList(DialogueConstants.MainContainerStyleSheet);
            extensionContainer.AddToClassList(DialogueConstants.ExternalContainerStyleSheet);
            _defaultBorderWidth = mainContainer.style.borderBottomWidth;
            _defaultBorderColor = mainContainer.style.borderBottomColor;
        }

        public virtual void Draw() 
        {
            //Title
            TextField nameTF = DialogueSystemUtils.CreateTextField(DialogueName, null, callback => 
            {
                TextField target = (TextField)callback.target;
                target.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();
                
                if (string.IsNullOrEmpty(target.value))
                {
                    if (!string.IsNullOrEmpty(DialogueName))
                    {
                        ++_graphView.HasError;
                    }
                }
                else 
                {
                    if (string.IsNullOrEmpty(DialogueName))
                    {
                        --_graphView.HasError;
                    }
                }

                if (Group == null) 
                {
                    _graphView.RemoveUngroupedNode(this);
                    DialogueName = target.value;
                    _graphView.AddUnGroupedNode(this);
                    return;
                }
                CustomGroup currentGroup = Group;
                _graphView.RemoveGroupedNode(this, Group);
                DialogueName = target.value;
                _graphView.AddGroupedNode(this, currentGroup);
            });
            UpdateStyle();
            nameTF.AddClasses(
                DialogueConstants.TextFieldStyleSheet,
                DialogueConstants.TextFieldHiddenStyleSheet,
                DialogueConstants.TextFieldFilenameStyleSheet
            );

            titleContainer.Insert(0, nameTF);

            //Input

            Port input = this.CreatePort(DialogueConstants.BaseNodeDialogueConnectionDefaultValue, Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);

            inputContainer.Add(input);

            //Ext. Container
            VisualElement customDC = new VisualElement();

            customDC.AddToClassList(DialogueConstants.MainContainerStyleSheet);

            Foldout textF = DialogueSystemUtils.CreateFoldout(DialogueConstants.BaseNodeQuoteDefaultValue);

            TextField textTF = DialogueSystemUtils.CreateTextArea(Text, null, callback => 
            {
                Text = callback.newValue;
            });

            textTF.AddClasses(
                DialogueConstants.TextFieldStyleSheet,
                DialogueConstants.TextFieldQuoteStyleSheet
            );

            ImageField.AddClasses(
                DialogueConstants.ObjectFieldStyleSheet
            );
            
            VoiceLineField.AddClasses(
                DialogueConstants.ObjectFieldStyleSheet
            );
            
            DialogueColorField.AddClasses(
                DialogueConstants.ColorFieldStyleSheet
            );


            textF.Add(textTF);
            customDC.Add(DialogueColorField);
            customDC.Add(ImageField);
            customDC.Add(textF);
            customDC.Add(VoiceLineField);
            extensionContainer.Add(customDC);
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction(DialogueConstants.DisconnectInputText, actionEvents => DisconnectPorts(inputContainer));
            evt.menu.AppendAction(DialogueConstants.DisconnectOutputText, actionEvents => DisconnectPorts(outputContainer));
            base.BuildContextualMenu(evt);
        }

        public void DisconnectAllPorts() 
        {
            DisconnectPorts(inputContainer);
            DisconnectPorts(outputContainer);
        }
        
        private void DisconnectPorts(VisualElement container) 
        {
            foreach (Port port in container.Children()) 
            {
                if (port.connected) 
                {
                    _graphView.DeleteElements(port.connections);
                }
            }
        }

        public bool IsStartingNode() 
        {
            Port port = (Port) inputContainer.Children().First();
            return !port.connected;
        }

        public void SetErrorStyle(Color color) 
        {
            mainContainer.style.borderTopColor = color;
            mainContainer.style.borderLeftColor = color;
            mainContainer.style.borderRightColor = color;
            mainContainer.style.borderBottomColor = color;
            mainContainer.style.borderTopWidth = 2f;
            mainContainer.style.borderLeftWidth = 2f;
            mainContainer.style.borderRightWidth =2f;
            mainContainer.style.borderBottomWidth = 2f;
        }

        public void ResetStyleError() 

        {
            mainContainer.style.borderTopColor = _defaultBorderColor;
            mainContainer.style.borderLeftColor = _defaultBorderColor;
            mainContainer.style.borderRightColor = _defaultBorderColor;
            mainContainer.style.borderBottomColor = _defaultBorderColor;
            mainContainer.style.borderTopWidth = _defaultBorderWidth;
            mainContainer.style.borderLeftWidth = _defaultBorderWidth;
            mainContainer.style.borderRightWidth = _defaultBorderWidth;
            mainContainer.style.borderBottomWidth = _defaultBorderWidth;
        }

        public void UpdateStyle()
        {
            mainContainer.style.backgroundColor = DialogueColorField.value;
        }
    }   
}
