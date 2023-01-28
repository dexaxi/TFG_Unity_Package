using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
//  Editor component that snaps objects to a custom grid.
/// </summary>
[ExecuteInEditMode]
public class SnapToGrid : MonoBehaviour
{
#if UNITY_EDITOR
    

    [Tooltip("Maximum size of cells in bot axis.")]
    const int _cellMaxSize = 100;
    
    [Header("Cell Sizes")]
    [SerializeField]
    [Range(0.1f, _cellMaxSize)]
    [Tooltip("Size of cells in the X axis.")]
    private float XcellSize;
    
    [SerializeField]
    [Range(0.1f, _cellMaxSize)]
    [Tooltip("Size of cells in the X axis.")]
    private float ZcellSize;

    [Space(15)]

    [Header("Height Parameters")]
    [SerializeField] 
    [Tooltip("Fixed height for elements in the grid.")]
    private bool snapToHeight;
    [SerializeField] 
    [Tooltip("Fixed height for elements in the grid.")]
    private float height;

    [Space(15)]

    [Header("Settings")]
    [SerializeField] 
    [Tooltip("Use local position instead of world position.")]
    private bool localPosition;

    private void Update()
    {
        Vector3 position;

        if (localPosition)
            position = transform.localPosition;
        else
            position = transform.position;

        if (XcellSize != 0)
            position.x = Mathf.Round(position.x / XcellSize) * XcellSize;

        if (snapToHeight)
            position.y = height;

        if (ZcellSize != 0)
            position.z = Mathf.Round(position.z / ZcellSize) * ZcellSize;


        if (localPosition)
            transform.localPosition = position;
        else
            transform.position = position;
    }

#endif
}