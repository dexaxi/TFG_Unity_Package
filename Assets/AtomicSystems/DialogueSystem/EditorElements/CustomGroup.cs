namespace DUJAL.Systems.Dialogue 
{
    using System;
    using UnityEditor.Experimental.GraphView;
    using UnityEngine;
    public class CustomGroup : Group
    {
        private float _defaultBorderWidth;
        private Color _defaultBorderColor;

        public string ID { get; set; }
        public string PreviousTitle { get; set; }

        public CustomGroup(string title, Vector2 position) : base()
        {
            ID = Guid.NewGuid().ToString();
            this.title = title;
            PreviousTitle = title;
            SetPosition(new Rect(position, Vector2.zero));
            _defaultBorderWidth = contentContainer.style.borderBottomWidth.value;
            _defaultBorderColor = contentContainer.style.borderBottomColor.value;
        }

        public void SetErrorStyle(Color color) 
        {
            contentContainer.style.borderBottomWidth = 4f;
            contentContainer.style.borderTopWidth = 4f;
            contentContainer.style.borderLeftWidth= 4f;
            contentContainer.style.borderRightWidth = 4f;
            contentContainer.style.borderBottomColor = color;
            contentContainer.style.borderTopColor = color;
            contentContainer.style.borderLeftColor = color;
            contentContainer.style.borderRightColor = color;
        }

        public void ResetStyle()
        {
            contentContainer.style.borderBottomWidth = _defaultBorderWidth;
            contentContainer.style.borderTopWidth = _defaultBorderWidth;
            contentContainer.style.borderLeftWidth = _defaultBorderWidth;
            contentContainer.style.borderRightWidth = _defaultBorderWidth;
            contentContainer.style.borderBottomColor = _defaultBorderColor;
            contentContainer.style.borderTopColor = _defaultBorderColor;
            contentContainer.style.borderLeftColor = _defaultBorderColor;
            contentContainer.style.borderRightColor = _defaultBorderColor;
        }
    }
}
