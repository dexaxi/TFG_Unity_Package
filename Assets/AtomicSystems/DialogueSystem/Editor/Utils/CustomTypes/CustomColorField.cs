namespace DUJAL.Systems.Utils 
{
    using UnityEngine;
    using DUJAL.Systems.Dialogue.Constants;
    using UnityEditor.UIElements;

    public class CustomColorField : ColorField
    {
        public delegate void Notify();
        public event Notify Changed;

        private Color _color;

        public CustomColorField() : base()
        {
            _color = DialogueConstants.DefaultNodeBackgroundColor;
        }
        public override Color value
        {
            get => _color;
            set
            {
                Changed.Invoke();
                _color = value;
            }
        }
    }
}
