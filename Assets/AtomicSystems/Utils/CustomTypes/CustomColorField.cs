using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using DUJAL.Systems.Dialogue.Constants;

namespace DUJAL.Systems.Utils 
{
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
