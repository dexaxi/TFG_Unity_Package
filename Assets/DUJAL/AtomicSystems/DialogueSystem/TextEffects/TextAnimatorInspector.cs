namespace DUJAL.Systems.Dialogue 
{
    using DUJAL.Systems.Dialogue.Constants;
    using System.Collections.Generic;
    using UnityEngine;
    using TMPro;
    using System;

    public class TextAnimatorInspector : MonoBehaviour
    {
        private TextMeshProUGUI _textComponent;
        
        private List<TextEffect> _effectReferences = new();

        public void HandleTextEffects(List<EffectInstance> effectInstances, TextMeshProUGUI text) 
        {
            _textComponent = text;

            foreach (EffectInstance effect in effectInstances)
            {
                TextEffects effectType = GetEnumFromTag(effect.Tag);
                if (effectType != TextEffects.Invalid)
                {
                    HandleEffect(effect, GetTypeFromEffect(effectType));
                }
            }
        }

        public void StopTextEffects() 
        {
            foreach (TextEffect effect in _effectReferences) 
            {
                effect.StopAnimation();
                Destroy(effect);
            }
            _effectReferences.Clear();
        }

        private TextEffects GetEnumFromTag(string tag)
        {
            if (tag.Contains(DialogueConstants.wobbleTag)) return TextEffects.Wobble;
            else if (tag.Contains(DialogueConstants.rainbowTag)) return TextEffects.Rainbow;

            return TextEffects.Invalid;
        }

        private Type GetTypeFromEffect(TextEffects effect) 
        {
            switch (effect) 
            {
                case TextEffects.Wobble:
                    return typeof(WobbleText);
                
                case TextEffects.Rainbow:
                    return null;
                
                default: 
                    return null;
            }
        }

        private void HandleEffect(EffectInstance effectInstance, Type effectType) 
        {
            //apply effect to substring
            var textEffect = gameObject.AddComponent(effectType) as TextEffect;
            textEffect.UpdateData(effectInstance, _textComponent);
            textEffect.StartAnimation();
            _effectReferences.Add(textEffect);
        }
    }
}