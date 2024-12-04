namespace DUJAL.Systems.Dialogue.Animations
{
    using UnityEngine;
    using DUJAL.Systems.Dialogue.Types;

    public class JitterText : TextEffect
    {
        public override void UpdateEffect()
        {
            if (!_doAnimate || _animationHandler.TextInfo == null)
            {
                return;
            }

            foreach (EffectInstance effect in _effects)
            {
                for (int i = effect.TextStartIdx; i < effect.GetTextEndIndex(); ++i)
                {
                    var charInfo = _animationHandler.TextInfo.characterInfo[i];
                    if (!charInfo.isVisible)
                    {
                        continue;
                    }

                    var meshInfo = _animationHandler.TextInfo.meshInfo[charInfo.materialReferenceIndex];
                    var verts = meshInfo.vertices;
                    for (int j = 0; j < 4; ++j)
                    {
                        int vertexIdx = charInfo.vertexIndex + j;
                        float newOrigY = Mathf.Sin(Time.time * 2f + verts[vertexIdx].x * 0.01f) * 10f;
                        verts[vertexIdx] = verts[vertexIdx] + new Vector3(0f, newOrigY, 0f);
                    }
                }
            }
        }
    }
}
