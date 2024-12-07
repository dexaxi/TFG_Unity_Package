namespace DUJAL.Systems.Dialogue.Animations
{
    using Dialogue.Types;

    using UnityEngine;
    using TMPro;

    using System.Collections.Generic;

    public class TextEffect : MonoBehaviour
    {
        protected List<EffectInstance> effects = new();
        protected TextMeshProUGUI textComponent;
        protected bool doAnimate;
        protected TextAnimationHandler animationHandler;
        
        public bool PauseUpdate { get; private set; }

        public virtual void UpdateData(EffectInstance effect, TextMeshProUGUI textComponent, TextAnimationHandler animatorInspector)
        {
            effects.Add(effect);
            this.textComponent = textComponent;
            animationHandler = animatorInspector;
            GetParamsFromTag();
        }

        public virtual void StartAnimation()
        {
            doAnimate = true;
        }

        public virtual void StopAnimation()
        {
            doAnimate = false;
            effects.Clear();
        }
        
        public virtual void GetParamsFromTag() { }

        public virtual void UpdateEffect() { }
    }
}