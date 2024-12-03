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
                Type type = TextEffectUtils.GetTypeFromEffect(effect.tagType);
                if (type == null) continue;
                HandleEffect(effect, type);
            }
        }

        public void StopTextEffects() 
        {
            foreach (TextEffect effect in _effectReferences) 
            {
                effect.StopAnimation();
            }
        }

        private void HandleEffect(EffectInstance effectInstance, Type effectType) 
        {
            //apply effect to substring
            var textEffect = gameObject.GetComponent(effectType) as TextEffect;
            
            if (textEffect == null)
            {
                textEffect = gameObject.AddComponent(effectType) as TextEffect;
                _effectReferences.Add(textEffect);
            }

            textEffect.UpdateData(effectInstance, _textComponent);
            textEffect.StartAnimation();
        }
    }
}