using DUJAL.Systems.Dialogue.Constants;
using DUJAL.Systems.Dialogue.Utils;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace DUJAL.Systems.Dialogue {
    public class MultipleChoiceNode : BaseNode
    {
        public override void Initialize(DialogueSystemGraphView graphView, Vector2 pos)
        {
            base.Initialize(graphView, pos);
            DialogueType = DialogueType.SingleChoice;
            ChoiceSaveData choiceData = new ChoiceSaveData()
            {
                Text = DialogueConstants.MultipleChoiceNewChoicDefaultText
            };
            Choices.Add(choiceData);
        }
        public override void Draw()
        {
            base.Draw();

            Button addChoice = DialogueSystemUtils.CreateButon(DialogueConstants.MultipleChoiceAddChoicDefaultText, () =>
            {
                ChoiceSaveData choiceData = new ChoiceSaveData()
                {
                    Text = DialogueConstants.MultipleChoiceNewChoicDefaultText 
                };
                Choices.Add(choiceData);
                outputContainer.Add(CreateNewChoice(choiceData));
            });

            addChoice.AddToClassList(DialogueConstants.ButtonStyleSheet);

            titleContainer.Insert(2, addChoice);

            foreach (ChoiceSaveData choice in Choices)
            {
                outputContainer.Add(CreateNewChoice(choice));
            }
            RefreshExpandedState();
        }

        private Port CreateNewChoice(object userData) 
        {
            Port outputChoice = this.CreatePort();
            outputChoice.userData = userData;
            ChoiceSaveData choiceData = (ChoiceSaveData) userData;
            Button delete = DialogueSystemUtils.CreateButon(DialogueConstants.MultipleChoiceXText, () => 
            {
                if (Choices.Count == 1) 
                {
                    return;
                }

                if (outputChoice.connected) 
                {
                    _graphView.DeleteElements(outputChoice.connections);
                }
                Choices.Remove(choiceData);
                _graphView.RemoveElement(outputChoice);
            });

            delete.AddToClassList(DialogueConstants.ButtonStyleSheet);

            TextField choiceTF = DialogueSystemUtils.CreateTextField(choiceData.Text, null, callback => 
            {
                choiceData.Text = callback.newValue; 
            });

            choiceTF.AddClasses(
                DialogueConstants.TextFieldStyleSheet,
                DialogueConstants.TextFieldHiddenStyleSheet,
                DialogueConstants.TextFieldChoiceStyleSheet
            );
            outputChoice.Add(choiceTF);
            outputChoice.Add(delete);
            return outputChoice;
        }
    }
}
