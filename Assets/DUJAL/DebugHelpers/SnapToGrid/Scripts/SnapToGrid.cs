namespace DUJAL.Debug.SnapToGrid 
{
    using UnityEngine;

    [ExecuteInEditMode]
    public class SnapToGrid : MonoBehaviour
    {
    #if UNITY_EDITOR_64
        const int _cellMaxSize = 100;
        [Space(3)]
        [Range(0.1f, _cellMaxSize)]
        public float XcellSize;

        [Space(3)]
        [Range(0.1f, _cellMaxSize)]
        public float YcellSize;

        [Space(3)]
        [Range(0.1f, _cellMaxSize)]
        public float ZcellSize;

        [Space(3)]
        [Header("Settings")]
        [SerializeField] bool _snapToHeight;
        [SerializeField] bool _localPosition;


        private void Update()
        {
            Vector3 position;

            if (_localPosition)
                position = transform.localPosition;
            else
                position = transform.position;

            if (XcellSize != 0)
                position.x = Mathf.Round(position.x / XcellSize) * XcellSize;

            if (_snapToHeight && YcellSize != 0)
                position.y = Mathf.Round(position.y / YcellSize) * YcellSize;

            if (ZcellSize != 0)
                position.z = Mathf.Round(position.z / ZcellSize) * ZcellSize;


            if (_localPosition)
                transform.localPosition = position;
            else
                transform.position = position;


        }

    #endif
    }
}
