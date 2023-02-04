using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DUJAL.Systems.Dialogue {
    public class MultipleChoiceNode : BaseNode
    {
        public override void Initialize(Vector2 pos)
        {
            base.Initialize(pos);
            DialogueType = DialogueType.SingleChoice;
            Choices.Add("Next Dialogue");
        }
        public override void Draw()
        {
            base.Draw();

            Button addChoice = new Button()
            {
                text = "Add Choice"
            };

            mainContainer.Insert(1, addChoice);

            foreach (string choice in Choices)
            {
                Port outputChoice = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
                outputChoice.portName = "";

                Button delete = new Button()
                {
                    text = "X"
                };

                TextField choiceTF = new TextField()
                {
                    value = choice
                };
                outputChoice.Add(choiceTF);
                outputChoice.Add(delete);

                outputContainer.Add(outputChoice);
            }
            RefreshExpandedState();
        }
    }
}
