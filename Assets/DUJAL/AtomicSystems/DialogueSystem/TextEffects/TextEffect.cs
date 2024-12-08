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
        
        // Can be overriden on new effect types to update an effect when it is added.
        public virtual void UpdateData(EffectInstance effect, TextMeshProUGUI textComponent, TextAnimationHandler animatorInspector)
        {
            effects.Add(effect);
            this.textComponent = textComponent;
            animationHandler = animatorInspector;
            GetParamsFromTag();
        }

        // Override to declare stuff whenever the animation is started
        public virtual void StartAnimation()
        {
            doAnimate = true;
        }

        // Override to clear dictionaries/flags when an animation is stopped or reset.
        public virtual void StopAnimation()
        {
            doAnimate = false;
            effects.Clear();
        }
        
        // Pure virtual function. Override on new text effects to read the params declared in DialogueConstants.
        public virtual void GetParamsFromTag() { }

        // Pure virtual function. Override on new text effects to handle the update loop for it. See examples on Rainbow/Jitter/Wobble/FadeIn.cs.
        public virtual void UpdateEffect() { }
    }
}