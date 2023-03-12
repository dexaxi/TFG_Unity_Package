using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DUJAL.Systems.Dialogue 
{
    public class CustomGroup : Group
    {
        private float defaultBorderWidth;
        private Color defaultBorderColor;

        public string PreviousTitle;

        public CustomGroup(string title, Vector2 position) : base()
        {
            this.title = title;
            this.PreviousTitle = title;
            SetPosition(new Rect(position, Vector2.zero));
            defaultBorderWidth = contentContainer.style.borderBottomWidth.value;
            defaultBorderColor = contentContainer.style.borderBottomColor.value;
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
            contentContainer.style.borderBottomWidth = defaultBorderWidth;
            contentContainer.style.borderTopWidth = defaultBorderWidth;
            contentContainer.style.borderLeftWidth = defaultBorderWidth;
            contentContainer.style.borderRightWidth = defaultBorderWidth;
            contentContainer.style.borderBottomColor = defaultBorderColor;
            contentContainer.style.borderTopColor = defaultBorderColor;
            contentContainer.style.borderLeftColor = defaultBorderColor;
            contentContainer.style.borderRightColor = defaultBorderColor;
        }
    }
}
