namespace DUJAL.Systems.Dialogue
{
    using System.Collections.Generic;
    using UnityEngine;
    using TMPro;
    public class TextEffect : MonoBehaviour
    {
        protected List<EffectInstance> _effects = new();
        protected TextMeshProUGUI _textComponent;
        protected bool _doAnimate;

        public void UpdateData(EffectInstance effect, TextMeshProUGUI textComponent)
        {
            _effects.Add(effect);
            _textComponent = textComponent;
        }

        public void StartAnimation()
        {
            _doAnimate = true;
        }

        public void StopAnimation()
        {
            _doAnimate = false;
            _effects.Clear();
        }

        protected void UpdateGeometry(TMP_TextInfo textInfo) 
        {
            for (int i = 0; i < textInfo.meshInfo.Length; ++i)
            {
                var meshInfo = textInfo.meshInfo[i];
                meshInfo.mesh.vertices = meshInfo.vertices;
                _textComponent.UpdateGeometry(meshInfo.mesh, i);
            }
        }
    }
}