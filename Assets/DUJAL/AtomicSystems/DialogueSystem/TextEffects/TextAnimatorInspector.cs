namespace DUJAL.Systems.Dialogue 
{
    using DUJAL.Systems.Dialogue.Constants;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using TMPro;
    using System;

    public enum TextEffects 
    {
        TextEffects_Wobble,
        TextEffects_Rainbow
    }

    public class TextAnimatorInspector : MonoBehaviour
    {
        TextMeshProUGUI textComponent;
        public List<int> nextOpenTags;

        private void Awake()
        {
            nextOpenTags = new List<int>();
            for (int i = 0; i < Enum.GetNames(typeof(TextEffects)).Length; i++)
            {
                nextOpenTags.Add(int.MaxValue);
            }
        }

        public void HandleTextEffects(TextMeshProUGUI text) 
        {
            textComponent = text;
            int indexOfNextCloseTag = textComponent.text.IndexOf(DialogueConstants.CloseTag);
            int indexOfNextOpenTag = CalculateNextOpenTag(indexOfNextCloseTag, out string nextEffect);
            
            if (indexOfNextCloseTag != -1 && indexOfNextOpenTag > indexOfNextCloseTag)
            {
                textComponent.text = textComponent.text.Remove(indexOfNextCloseTag, DialogueConstants.CloseTag.Length);
            }
            else
            {
                EffectSwitch(nextEffect);
            }
        }

        private int CalculateNextOpenTag(int nextClosedTag, out string nextEffect)
        {
            int aux = int.MaxValue;
            int indexAux = 0;
            for (int i = 0; i < nextOpenTags.Count; i++) 
            {
                nextOpenTags[i] = textComponent.text.IndexOf(GetStringFromEffect((TextEffects)i));
            }
            
            for (int i = 0; i < nextOpenTags.Count; i++)
            {
                if (nextOpenTags[i] != -1 && nextOpenTags[i] < aux) 
                {
                    aux = nextOpenTags[i];
                    indexAux = i;
                }
            }

            nextEffect = GetStringFromEffect((TextEffects)indexAux);
            return aux;
        }

        private string GetStringFromEffect(TextEffects effect) 
        {
            switch (effect) 
            {
                case TextEffects.TextEffects_Wobble:
                    return DialogueConstants.wobbleTag;
                
                case TextEffects.TextEffects_Rainbow:
                    return DialogueConstants.rainbowTag;
                
                default: 
                    return "";
            }
        }

        private void EffectSwitch(string nextEffect)
        {
            switch (nextEffect)
            {
                case DialogueConstants.wobbleTag:
                    HandleWobble();
                    break;
                case DialogueConstants.rainbowTag:
                    HandleRainbow();
                    break;
            }
        }

        private void HandleWobble() 
        {
            string tag = DialogueConstants.wobbleTag;
            if (textComponent.text.Contains(tag))
            {
                //apply effect to substring
                int indexOfNextCloseTag = textComponent.text.IndexOf(DialogueConstants.CloseTag);
                int applicableStartIndex = textComponent.text.IndexOf(tag) + tag.Length;
                StartCoroutine(WobbleText.WobbleTextC(textComponent, applicableStartIndex, indexOfNextCloseTag));
                RemoveExtraText(tag);
            }
        }

        private void HandleRainbow()
        {
            string tag = DialogueConstants.rainbowTag;
            if (textComponent.text.Contains(tag))
            {
                //apply effect to substring
                int indexOfNextCloseTag = textComponent.text.IndexOf(DialogueConstants.CloseTag);
                int applicableStartIndex = textComponent.text.IndexOf(tag) + tag.Length;
                //
                RemoveExtraText(tag);
            }
        }

        private void RemoveExtraText(string tag)
        {
            textComponent.text = textComponent.text.Remove(textComponent.text.IndexOf(tag), tag.Length);
        }
    }
}