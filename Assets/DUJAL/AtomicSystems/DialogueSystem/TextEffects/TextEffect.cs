namespace DUJAL.Systems.Dialogue
{
    using UnityEngine;
    using TMPro;

    public class TextEffect : MonoBehaviour
    {
        protected EffectInstance _effect;
        protected TextMeshProUGUI _textComponent;
        protected bool _doAnimate;

        private void Awake()
        {
            _doAnimate = false;
        }

        public void UpdateData(EffectInstance effect, TextMeshProUGUI textComponent)
        {
            _effect = effect;
            _textComponent = textComponent;
        }

        public void StartAnimation()
        {
            _doAnimate = true;
        }

        public void StopAnimation()
        {
            _doAnimate = false;
        }
    }
}