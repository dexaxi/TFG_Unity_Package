namespace DUJAL.Systems.Dialogue
{
    using System.Collections.Generic;
    using UnityEngine;
    using TMPro;
    using Cysharp.Threading.Tasks;

    public class TextEffect : MonoBehaviour
    {
        protected List<EffectInstance> _effects = new();
        protected TextMeshProUGUI _textComponent;
        protected bool _doAnimate;
        protected TextAnimatorInspector _animationHandler;

        public virtual void UpdateData(EffectInstance effect, TextMeshProUGUI textComponent, TextAnimatorInspector animatorInspector)
        {
            _effects.Add(effect);
            _textComponent = textComponent;
            _animationHandler = animatorInspector;
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

        public virtual void UpdateEffect() { }
    }
}