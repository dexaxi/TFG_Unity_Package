
namespace DUJAL.Debug.DrawColliders 
{
    using UnityEngine;

    /// <summary>
    //  Class Component that allows you to print a BoxCollider2D in game using a LineRenderer
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class DrawBoxCollider2D : MonoBehaviour
    {
        [Header("Assets and Parameters")]
        [SerializeField] 
        [Tooltip("Use the Line Prefab Asset that comes with the package.")]
        public GameObject LinePrefab;
    
        [SerializeField] 
        [Tooltip("Color of the collider border you want printed")]
        public Color LineColor;

        private LineRenderer _line;
        private BoxCollider2D _boxCol;

        void Start()
        {
            _line = Instantiate(LinePrefab).GetComponent<LineRenderer>();
            _line.startColor = LineColor;
            _line.endColor = LineColor;
            _line.transform.SetParent(transform);
            _line.transform.localPosition = Vector3.zero;
            _boxCol = GetComponent<BoxCollider2D>();

        }

        void Update()
        {
            HighlightCollider();
        }

        /// <summary>
        //  Void Function to print a Box Collider 2D as a Line Renderer while in game.
        /// </summary>
        void HighlightCollider()
        {
            Vector3[] pos = new Vector3[4];
            pos[0] = transform.TransformPoint(new Vector3(_boxCol.size.x / 2.0f, _boxCol.size.y / 2.0f, 0));
            pos[1] = transform.TransformPoint(new Vector3(-_boxCol.size.x / 2.0f, _boxCol.size.y / 2.0f, 0));
            pos[2] = transform.TransformPoint(new Vector3(-_boxCol.size.x / 2.0f, -_boxCol.size.y / 2.0f, 0));
            pos[3] = transform.TransformPoint(new Vector3(_boxCol.size.x / 2.0f, -_boxCol.size.y / 2.0f, 0));
            _line.SetPositions(pos);
        }
    }
}