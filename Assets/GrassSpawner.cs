using UnityEngine;

/// <summary>
/// Procedurally spawns a grass prefab across a plane.
/// Attach this script to any GameObject in your scene.
/// </summary>
public class GrassSpawner : MonoBehaviour
{
    [Header("Grass Prefab")]
    [Tooltip("Drag your grass FBX prefab here")]
    public GameObject grassPrefab;

    [Header("Plane Settings")]
    [Tooltip("The plane to cover with grass (leave null to use a flat area defined below)")]
    public GameObject targetPlane;

    [Tooltip("Width of the area to cover (X axis) — used if no plane is assigned")]
    public float areaWidth = 50f;

    [Tooltip("Depth of the area to cover (Z axis) — used if no plane is assigned")]
    public float areaDepth = 50f;

    [Tooltip("Center of the spawn area in world space")]
    public Vector3 areaCenter = Vector3.zero;

    [Header("Density & Spacing")]
    [Tooltip("Minimum distance between each grass patch")]
    public float spacing = 1.5f;

    [Tooltip("How much random offset to apply to each patch position (0 = perfect grid)")]
    [Range(0f, 1f)]
    public float randomOffset = 0.6f;

    [Header("Random Rotation")]
    public bool randomYRotation = true;

    [Header("Random Scale")]
    public bool randomScale = true;
    public float minScale = 80f;
    public float maxScale = 120f;

    [Header("Ground Snapping")]
    [Tooltip("Snap grass to the ground using a raycast")]
    public bool snapToGround = true;
    public LayerMask groundLayer = ~0; // Everything by default
    public float raycastHeight = 10f;

    [Header("Performance")]
    [Tooltip("Parent all grass under a single GameObject to keep the hierarchy clean")]
    public bool useParentObject = true;

    private GameObject _grassParent;

    void Start()
    {
        SpawnGrass();
    }

    /// <summary>
    /// Call this to (re)generate grass at runtime.
    /// </summary>
    public void SpawnGrass()
    {
        if (grassPrefab == null)
        {
            Debug.LogError("[GrassSpawner] No grass prefab assigned!");
            return;
        }

        // Clean up previous grass
        ClearGrass();

        // Determine spawn bounds
        float width = areaWidth;
        float depth = areaDepth;
        Vector3 center = areaCenter;

        if (targetPlane != null)
        {
            Renderer r = targetPlane.GetComponent<Renderer>();
            if (r != null)
            {
                width = r.bounds.size.x;
                depth = r.bounds.size.z;
                center = r.bounds.center;
            }
        }

        // Create parent
        if (useParentObject)
        {
            _grassParent = new GameObject("GrassParent");
            _grassParent.transform.position = Vector3.zero;
        }

        float halfW = width / 2f;
        float halfD = depth / 2f;

        int count = 0;

        for (float x = -halfW; x < halfW; x += spacing)
        {
            for (float z = -halfD; z < halfD; z += spacing)
            {
                // Add random jitter so it doesn't look like a grid
                float jitterX = Random.Range(-spacing, spacing) * randomOffset;
                float jitterZ = Random.Range(-spacing, spacing) * randomOffset;

                Vector3 spawnPos = new Vector3(
                    center.x + x + jitterX,
                    center.y,
                    center.z + z + jitterZ
                );

                // Ground snap via raycast
                if (snapToGround)
                {
                    Ray ray = new Ray(spawnPos + Vector3.up * raycastHeight, Vector3.down);
                    if (Physics.Raycast(ray, out RaycastHit hit, raycastHeight * 2f, groundLayer))
                    {
                        spawnPos.y = hit.point.y;
                    }
                }

                // Rotation — 180 X flip corrects the upside-down FBX export axis
                Quaternion rot = Quaternion.Euler(180f, randomYRotation ? Random.Range(0f, 360f) : 0f, 0f);

                // Scale
                Vector3 scale = Vector3.one;
                if (randomScale)
                {
                    float s = Random.Range(minScale, maxScale);
                    scale = new Vector3(s, s, s);
                }

                GameObject grass = Instantiate(grassPrefab, spawnPos, rot,
                    useParentObject ? _grassParent.transform : transform);
                grass.transform.localScale = scale;
                count++;
            }
        }

        Debug.Log($"[GrassSpawner] Spawned {count} grass patches.");
    }

    /// <summary>
    /// Destroys all previously spawned grass.
    /// </summary>
    public void ClearGrass()
    {
        if (_grassParent != null)
        {
            DestroyImmediate(_grassParent);
            _grassParent = null;
        }
    }

#if UNITY_EDITOR
    // Draw the spawn area in the Scene view
    void OnDrawGizmosSelected()
    {
        float w = areaWidth;
        float d = areaDepth;
        Vector3 c = areaCenter;

        if (targetPlane != null)
        {
            Renderer r = targetPlane.GetComponent<Renderer>();
            if (r != null) { w = r.bounds.size.x; d = r.bounds.size.z; c = r.bounds.center; }
        }

        Gizmos.color = new Color(0.2f, 0.9f, 0.2f, 0.3f);
        Gizmos.DrawCube(c, new Vector3(w, 0.05f, d));
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(c, new Vector3(w, 0.05f, d));
    }
#endif
}