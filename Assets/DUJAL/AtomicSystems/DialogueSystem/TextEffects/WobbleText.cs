namespace DUJAL.Systems.Dialogue 
{
    using UnityEngine;

    public class WobbleText : TextEffect
    {
        private void Update()
        {
            if (!_doAnimate) return;

            _textComponent.ForceMeshUpdate();
            var textInfo = _textComponent.textInfo;
            for (int i = _effect.TextStartIndex; i < _effect.GetTextEndIndex(); ++i)
            {
                var charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible)
                {
                    continue;
                }

                var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
                for (int j = 0; j < 4; ++j)
                {
                    var orig = verts[charInfo.vertexIndex + j];
                    float newOrigY = Mathf.Sin(Time.fixedTime * 2f + orig.x * 0.01f) * 10f;
                    verts[charInfo.vertexIndex + j] = orig + new Vector3(0f, newOrigY, 0f);
                }
            }

            for (int i = 0; i < textInfo.meshInfo.Length; ++i)
            {
                var meshInfo = textInfo.meshInfo[i];
                meshInfo.mesh.vertices = meshInfo.vertices;
                _textComponent.UpdateGeometry(meshInfo.mesh, i);
            }
        }
    }
}
