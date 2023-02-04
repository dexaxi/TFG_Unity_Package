using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DUJAL.IndependentComponents.ScreenShake
{

    /// <summary>
    //  Animation Curve Types to select.
    /// </summary>
    public enum AnimationCurveType
    {
        EaseIn = 0,
        EaseOut,
        EndKick,
        Kick,
        SoftKick,
        Squared,
        Stable,
    }


    /// <summary>
    //  Class to add to a prefab that will server as a loader for all the different AnimationCurveScriptableObjects
    /// </summary>
    public class AnimationCurveSelector : MonoBehaviour
    {
        public static AnimationCurveSelector Instance { get; private set; }

        [Header("Settings")]
        [SerializeField]
        [Tooltip("Toggle this if you want the Animation Curve Selector to not be destroyed when loading a new Scene")]
        private bool dontDestroyOnLoad;

        [Space(15)]

        [SerializeField]
        [Tooltip("Animation Curve Asset List, to add a new type of curve, add an asset to the list and add its name to the Animation Curve Type Enum")]
        private List<AnimationCurveAsset> animationCurveAssetList = new List<AnimationCurveAsset>();

        //Singleton
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                if (dontDestroyOnLoad) DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

        /// <summary>
        //  Get Curve from Animation Curve Type
        /// </summary>
        public AnimationCurve GetCurve(AnimationCurveType animationCurveType) => animationCurveAssetList[(int)animationCurveType];

    }
}