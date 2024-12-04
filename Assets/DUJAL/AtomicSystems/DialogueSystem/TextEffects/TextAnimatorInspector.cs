namespace DUJAL.Systems.Dialogue
{
    using System;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;

    public class TextAnimatorInspector : MonoBehaviour
    {
        private TextMeshProUGUI _textComponent;
        private List<TextEffect> _effectReferences = new();
        
        public TMP_TextInfo TextInfo;

        public void HandleTextEffects(List<EffectInstance> effectInstances, TextMeshProUGUI text) 
        {
            _textComponent = text;
            TextInfo = _textComponent.textInfo;
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

            textEffect.UpdateData(effectInstance, _textComponent, this);
            textEffect.StartAnimation();
        }

        private void Update()
        {
            if(_textComponent == null || TextInfo == null) return;

            _textComponent.ForceMeshUpdate();
            foreach (TextEffect effect in _effectReferences) 
            {
                effect.UpdateEffect();
            }

            UpdateGeometry();
        }
        public void UpdateGeometry()
        {
            for (int i = 0; i < TextInfo.meshInfo.Length; ++i)
            {
                TMP_MeshInfo meshInfo = TextInfo.meshInfo[i];
                meshInfo.mesh.vertices = meshInfo.vertices;
                meshInfo.mesh.colors32 = meshInfo.colors32;
                _textComponent.UpdateGeometry(meshInfo.mesh, i);
            }
        }
    }
}