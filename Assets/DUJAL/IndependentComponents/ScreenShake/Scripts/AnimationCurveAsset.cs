namespace DUJAL.IndependentComponents.ScreenShake
{
    using UnityEngine;
    /// <summary>
    //  Animation Curve Asset to store different types of curve.
    /// </summary>
    [CreateAssetMenu(fileName = "New Animation Curve", menuName = "DUJAL/Animation Curve")]
    public class AnimationCurveAsset : ScriptableObject
    {
        [Header("Animation Curve Scriptable Object Asset")]
        public AnimationCurve Curve = AnimationCurve.Linear(0, 0, 1, 1);

        public static implicit operator AnimationCurve(AnimationCurveAsset me)
        {
            return me.Curve;
        }
        public static implicit operator AnimationCurveAsset(AnimationCurve curve)
        {
            AnimationCurveAsset asset = ScriptableObject.CreateInstance<AnimationCurveAsset>();
            asset.Curve = curve;
            return asset;
        }
    }
}
