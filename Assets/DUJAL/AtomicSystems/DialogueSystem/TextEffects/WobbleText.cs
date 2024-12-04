namespace DUJAL.Systems.Dialogue 
{
    using Cysharp.Threading.Tasks;
    using System.Globalization;
    using System.Threading;
    using UnityEngine;

    public class WobbleText : TextEffect
    {
        public override void UpdateEffect()
        {
            if (!_doAnimate || _animationHandler.TextInfo == null) 
            {
                return;
            }

            foreach (EffectInstance effect in _effects) 
            {
                for (int i = effect.TextStartIndex; i < effect.GetTextEndIndex(); ++i)
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
                        int vertexIndex = charInfo.vertexIndex + j;
                        float newOrigY = Mathf.Sin(Time.time * 2f + verts[vertexIndex].x * 0.01f) * 10f;
                        verts[vertexIndex] = verts[vertexIndex] + new Vector3(0f, newOrigY, 0f);
                    }
                }
            }
        }
    }
}
