namespace DUJAL.Systems.Dialogue.Animations
{
    using Dialogue.Types;
    using Dialogue.Constants;
    using Dialogue.Animations.Utils;

    using UnityEngine;
    using TMPro;

    using System;
    using System.Collections.Generic;

    public class TextAnimationHandler : MonoBehaviour
    {
        private TextMeshProUGUI _textComponent;
        private List<TextEffect> _effectReferences = new();
        private TMP_MeshInfo _meshInfo;
        public TMP_TextInfo TextInfo;

        public void HandleTextEffects(List<EffectInstance> effectInstances, TextMeshProUGUI text) 
        {
            _textComponent = text;
            
            TextInfo = _textComponent.textInfo;
            
            // For each effect instance handle it.
            foreach (EffectInstance effect in effectInstances)
            {
                Type type = TextEffectUtils.GetTypeFromEffect(effect.tagType);
                if (type == null) continue;
                HandleEffect(effect, type);
            }
        }

        // Call to kill all text effects
        public void StopTextEffects() 
        {
            foreach (TextEffect effect in _effectReferences) 
            {
                effect.StopAnimation();
            }
        }

        // Called when dialogue ends
        public void EndAnimationHandling()
        {
            while (_effectReferences.Count > 0)
            {
                var currEffect = _effectReferences[0];
                _effectReferences.Remove(currEffect);
                Destroy(currEffect);
            }
        }

        // handle a particular effect instance
        private void HandleEffect(EffectInstance effectInstance, Type effectType) 
        {
            //apply effect to substring
            var textEffect = gameObject.GetComponent(effectType) as TextEffect;
            
            // If component does not exist add it.
            if (textEffect == null)
            {
                textEffect = gameObject.AddComponent(effectType) as TextEffect;
                // keep track of the references to destroy them later
                _effectReferences.Add(textEffect);
            }

            textEffect.UpdateData(effectInstance, _textComponent, this);
            textEffect.StartAnimation();
        }

        //Important to add it to the late update in order to correctly render TMPro info.
        private void LateUpdate()
        {
            if (_textComponent == null || TextInfo == null) return;
            
            _textComponent.ForceMeshUpdate();
            
            // All effects can be updated from here.
            foreach (TextEffect effect in _effectReferences) 
            {
                effect.UpdateEffect();
            }

            PreUpdateEffectGeometry();
            UpdateGeometry();
        }

        // Update _meshInfo with the updated color and vertex data. (Can add normal data and etc.. if needed for new animations.
        public void PreUpdateEffectGeometry()
        {
            for (int i = 0; i < TextInfo.meshInfo.Length; ++i)
            {
                _meshInfo = TextInfo.meshInfo[i];
                if (_meshInfo.mesh == null) continue;
                _meshInfo.mesh.vertices = _meshInfo.vertices;
                _meshInfo.mesh.colors32 = _meshInfo.colors32;
            }
        }

        // Mesh data is stored on a separate copy in "_meshInfo.mesh". Need to call UpdateGeometry to apply the changes gathered by the TextEffect system.
        public void UpdateGeometry()
        {
            if (_meshInfo.mesh == null) return;
            for (int i = 0; i < TextInfo.meshInfo.Length; ++i)
            {
                _textComponent.UpdateGeometry(_meshInfo.mesh, i);
            }
        }
    }
}