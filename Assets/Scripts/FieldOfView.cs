using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// A sensor representing the field of view for any given gameObject.
/// This was referenced from TheKiwiCoder's YouTube video about creating a line of sight checking using sensors:
/// https://www.youtube.com/watch?v=znZXmmyBF-o
/// </summary>
[ExecuteInEditMode]
public class FieldOfView : NetworkBehaviour
{
    [Header("Field of View Properties")]
    [SerializeField]
    private float _angle;

    [SerializeField]
    private float _distance;

    [SerializeField]
    private float _height;

    [SerializeField]
    private float _heightOffset;

    [SerializeField]
    private int _subdivisionFactor;

    [Header("Scanning Properties")]
    [SerializeField]
    private float _scanFrequency;

    [SerializeField]
    private LayerMask _interestedLayers;

    [SerializeField]
    private LayerMask _occlusionLayers;

    [Header("Debugging Properties")]
    [SerializeField]
    private Color _meshColor;

    [SerializeField]
    private Color _visibleObject;

    private Mesh _mesh;
    private List<GameObject> _detectedObjects = new List<GameObject>();
    private static int maxColliders = 10; // Define some arbitrary number to ensure we have enough space to store results from Physics operations.
    private Collider[] _detectedColliders = new Collider[maxColliders]; 
    private int _colliderCount;
    private float _scanInterval;
    private float _scanTimer;

    public List<GameObject> DetectedObjects
    {
        get
        {
            _detectedObjects.RemoveAll(obj => !obj); // Remove any null objects
            return _detectedObjects;
        }
    }

    /// <summary>
    /// Called once per frame to update the scan timer and periodically scan for objects within the vision cone's range.
    /// </summary>
    [Server]
    private void Update()
    {
        if(!Application.isPlaying) return;
        _scanTimer -= Time.deltaTime;

        if (_scanTimer < 0)
        {
            _scanTimer += _scanInterval;
            ScanObjects();
        }
    }

    /// <summary>
    /// Display boundaries of the vision cone in the scene view and highlight what objects it is able to see.
    /// </summary>
    private void OnDrawGizmos()
    {
        // Display the boundaries of the vision cone.
        if (_mesh)
        {
            Gizmos.color = _meshColor;
            Gizmos.DrawMesh(_mesh, transform.position, transform.rotation);
        }

        // Highlight what objects the vision cone is able to see.
        foreach (GameObject detectedObject in DetectedObjects)
        {
            Gizmos.color = _visibleObject;

            // We define some arbitrary radius to display the sphere and ensure that the sensor is working correctly.
            Gizmos.DrawSphere(detectedObject.transform.position, 0.15f);
        }
    }

    /// <summary>
    /// Called when the script is loaded or when a value changes in the inspector.
    /// </summary>
    protected override void OnValidate()
    {
        base.OnValidate();
        _mesh = CreateMesh();
        _scanInterval = 1 / _scanFrequency;
    }

    /// <summary>
    /// Dynamically create a vision cone mesh, represented as a wedge, using the input parameters given from the inspector.
    /// </summary>
    /// <returns>A dynamically generated vision cone mesh shaped as a wedge.</returns>
    private Mesh CreateMesh()
    {
        Mesh wedgeMesh = new Mesh();

        // To create a FOV mesh in the shape of a vision cone (aka wedge), we need a minimum of 8 triangles.
        // This can be derived from the 3D shape of a wedge where we require 2 triangles for the top and bottom, and 3 rectangles for the sides.
        // Since two triangles make up a rectangle, we're given with 2 + 2(3) = 8 triangles for a wedge-shaped mesh.

        // In order to create a custom, rounded edge at the far end of the vision cone we must subdivide by a given number, resulting in multiple, smaller wedges.
        // As we do not want to render the sides of each subwedge, we only account for the far end and the top and bottom faces.
        // Using the logic provided above, this results in 1(2) + 2 = 4.
        // Then, we must render the sides, which results in 2(2) = 4.
        int numTriangles = (_subdivisionFactor * 4) + 4;
        int numVertices = numTriangles * 3; // Since each triangle has 3 verticies, we multiply by 3.

        int[] meshTriangles = new int[numVertices];
        Vector3[] meshVertices = new Vector3[numVertices];
        
        Vector3 bottomCenter = Vector3.zero;
        Vector3 bottomRight = Quaternion.Euler(0f, _angle, 0f) * Vector3.forward * _distance;
        Vector3 bottomLeft = Quaternion.Euler(0f, -_angle, 0f) * Vector3.forward * _distance;

        bottomCenter.y = _heightOffset;
        bottomRight.y = _heightOffset;
        bottomLeft.y = _heightOffset;

        Vector3 topCenter = bottomCenter + Vector3.up * _height;
        Vector3 topRight = bottomRight + Vector3.up * _height;
        Vector3 topLeft = bottomLeft + Vector3.up * _height;

        // Create the vertex buffer for the dynamic mesh.
        // Triangles are to be sorted in a clockwise direction where the next triangle starts off at the previous triangle's endpoint to ensure the normals are pointing in the correct direction.
        int vertexIndex = 0;

        // Left Side
        meshVertices[vertexIndex++] = bottomCenter;
        meshVertices[vertexIndex++] = bottomLeft;
        meshVertices[vertexIndex++] = topLeft;

        meshVertices[vertexIndex++] = topLeft;
        meshVertices[vertexIndex++] = topCenter;
        meshVertices[vertexIndex++] = bottomCenter;

        // Right Side
        meshVertices[vertexIndex++] = bottomCenter;
        meshVertices[vertexIndex++] = topCenter;
        meshVertices[vertexIndex++] = topRight;

        meshVertices[vertexIndex++] = topRight;
        meshVertices[vertexIndex++] = bottomRight;
        meshVertices[vertexIndex++] = bottomCenter;

        // Wedge Segments
        float currentAngle = -_angle;
        float deltaAngle = (_angle * 2) / _subdivisionFactor; // Since we have to account for both the the left and right sides of the wedge segments from 0 degrees, we multiply by 2.

        for (int i = 0; i < _subdivisionFactor; ++i)
        {
            bottomRight = Quaternion.Euler(0f, currentAngle + deltaAngle, 0f) * Vector3.forward * _distance;
            bottomLeft = Quaternion.Euler(0f, currentAngle, 0f) * Vector3.forward * _distance;

            bottomRight.y = _heightOffset;
            bottomLeft.y = _heightOffset;

            topRight = bottomRight + Vector3.up * _height;
            topLeft = bottomLeft + Vector3.up * _height;

            // Far Side
            meshVertices[vertexIndex++] = bottomLeft;
            meshVertices[vertexIndex++] = bottomRight;
            meshVertices[vertexIndex++] = topRight;

            meshVertices[vertexIndex++] = topRight;
            meshVertices[vertexIndex++] = topLeft;
            meshVertices[vertexIndex++] = bottomLeft;

            // Top
            meshVertices[vertexIndex++] = topCenter;
            meshVertices[vertexIndex++] = topLeft;
            meshVertices[vertexIndex++] = topRight;

            // Bottom
            meshVertices[vertexIndex++] = bottomCenter;
            meshVertices[vertexIndex++] = bottomRight;
            meshVertices[vertexIndex++] = bottomLeft;

            currentAngle += deltaAngle;
        }

        // Setup an index buffer to map each vertex to its corresponding index.
        for (int i = 0; i < numVertices; ++i)
        {
            meshTriangles[i] = i;
        }

        wedgeMesh.vertices = meshVertices;
        wedgeMesh.triangles = meshTriangles;
        wedgeMesh.RecalculateNormals();

        return wedgeMesh;
    }

    /// <summary>
    /// Scan for objects within a certain radius of the current gameObject.
    /// </summary>
    [Server]
    private void ScanObjects()
    {
        _colliderCount = Physics.OverlapSphereNonAlloc(transform.position, _distance, _detectedColliders, _interestedLayers, QueryTriggerInteraction.Collide);
        _detectedObjects.Clear();

        for (int i = 0; i < _colliderCount; ++i)
        {
            GameObject detectedObject = _detectedColliders[i].gameObject;
            if (IsInSight(detectedObject))
            {
                _detectedObjects.Add(detectedObject.gameObject);
            }
        }
    }

    /// <summary>
    /// Check to see whether or not a gameObject is within the bounds of the vision cone.
    /// </summary>
    /// <param name="obj">The gameObject to check for.</param>
    /// <returns>Returns true if the object is within the vision cone boundaries; otherwise, false.</returns>
    public bool IsInSight(GameObject obj)
    {
        Vector3 origin = transform.position;
        Vector3 objectDestination = obj.transform.position;
        Vector3 objectDirection =  objectDestination - origin;

        // Check if it is within the height of the vision cone.
        if (objectDirection.y < _heightOffset || objectDirection.y > _height + _heightOffset)
        {
            return false;
        }

        // Check if it is within the angle range of the vision cone.
        objectDirection.y = 0;
        float deltaAngle = Vector3.Angle(objectDirection, transform.forward);

        if (_angle < deltaAngle)
        {
            return false;
        }

        // Use a raycast to check if there is an occlusion object in the way.
        origin.y += (_height / 2) + _heightOffset; // Ensure the raycast's origin height is in the center height of our vision cone
        objectDestination.y = origin.y;

        if (Physics.Linecast(origin, objectDestination, _occlusionLayers))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Filters detected objects by their layer name.
    /// </summary>
    /// <param name="targetObjects">A list of the target objects detected.</param>
    /// <param name="layerName">The layer name to filter by.</param>
    /// <returns>Returns the total number of filtered objects detected.</returns>
    public int FilterObjects(GameObject[] targetObjects, string layerName)
    {
        int layerNum = LayerMask.NameToLayer(layerName);
        int targetIndex = 0;

        foreach (var obj in _detectedObjects)
        {
            if (obj.layer == layerNum)
            {
                targetObjects[targetIndex++] = obj;
            }

            if (targetObjects.Length == targetIndex)
            {
                break; // Detected object buffer is full
            }
        }

        return targetIndex;
    }
}
