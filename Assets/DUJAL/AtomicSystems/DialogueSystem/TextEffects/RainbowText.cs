namespace DUJAL.Systems.Dialogue
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.XR;

    public class RainbowText : TextEffect
    {
        void Update()
        {
            if (!_doAnimate)
            {
                return;
            }

            _textComponent.ForceMeshUpdate();
            var textInfo = _textComponent.textInfo;
            foreach (EffectInstance effect in _effects)
            {
                for (int i = effect.TextStartIndex; i < effect.GetTextEndIndex(); ++i)
                {
                    var charInfo = textInfo.characterInfo[i];
                    if (!charInfo.isVisible)
                    {
                        continue;
                    }

                    var meshInfo = textInfo.meshInfo[charInfo.materialReferenceIndex];
                    for (int j = 0; j < 4; ++j)
                    {
                        int vertexIndex = charInfo.vertexIndex + j;
                        meshInfo.colors32[vertexIndex] = Color.red;
                    }
                }
            }

            UpdateGeometry(textInfo);
        }
    }
}