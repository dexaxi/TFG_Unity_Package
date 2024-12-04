namespace DUJAL.Systems.Dialogue.Animations
{
    using System.Collections.Generic;
    using UnityEngine;
    using TMPro;
    using DUJAL.Systems.Dialogue.Types;

    public class TextEffect : MonoBehaviour
    {
        protected List<EffectInstance> _effects = new();
        protected TextMeshProUGUI _textComponent;
        protected bool _doAnimate;
        protected TextAnimationHandler _animationHandler;

        public virtual void UpdateData(EffectInstance effect, TextMeshProUGUI textComponent, TextAnimationHandler animatorInspector)
        {
            _effects.Add(effect);
            _textComponent = textComponent;
            _animationHandler = animatorInspector;
            GetParamsFromTag();
        }

        public virtual void StartAnimation()
        {
            _doAnimate = true;
        }

        public virtual void StopAnimation()
        {
            _doAnimate = false;
            _effects.Clear();
        }
        
        public virtual void GetParamsFromTag() { }

        public virtual void UpdateEffect() { }
    }
}