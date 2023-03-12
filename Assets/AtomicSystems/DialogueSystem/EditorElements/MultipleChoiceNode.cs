using DUJAL.Systems.Dialogue.Constants;
using DUJAL.Systems.Dialogue.Utils;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DUJAL.Systems.Dialogue {
    public class MultipleChoiceNode : BaseNode
    {
        public override void Initialize(DialogueSystemGraphView graphView, Vector2 pos)
        {
            base.Initialize(graphView, pos);
            DialogueType = DialogueType.SingleChoice;
            Choices.Add(DialogueConstants.MultipleChoiceNewChoicDefaultText);
        }
        public override void Draw()
        {
            base.Draw();

            Button addChoice = DialogueSystemUtils.CreateButon(DialogueConstants.MultipleChoiceAddChoicDefaultText, () => 
            {
                outputContainer.Add(CreateNewChoice(DialogueConstants.MultipleChoiceNewChoicDefaultText));
            });

            addChoice.AddToClassList(DialogueConstants.ButtonStyleSheet);

            titleContainer.Insert(2, addChoice);

            foreach (string choice in Choices)
            {
                outputContainer.Add(CreateNewChoice(choice));
            }
            RefreshExpandedState();
        }

        private Port CreateNewChoice(string choice) 
        {
            Port outputChoice = this.CreatePort();

            Button delete = DialogueSystemUtils.CreateButon(DialogueConstants.MultipleChoiceXText);

            delete.AddToClassList(DialogueConstants.ButtonStyleSheet);

            TextField choiceTF = DialogueSystemUtils.CreateTextField(choice);

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
