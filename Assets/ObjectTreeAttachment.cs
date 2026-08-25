using UnityEngine;


public class ObjectTreeAttachment : MonoBehaviour
{
    [Header("Trigger-Bedingung")]

    public float proximityDistance = 2f;

    public Transform playerReference;

    [Header("Canvas")]
   
    public GameObject treeCanvasPrefab;
    [Tooltip("Position der Canvas relativ zum Objekt")]
    public Vector3 canvasOffset = new Vector3(0, 0.3f, 0);

    TreeMenuBuilder treeBuilder;

    void Awake()
    {
        if (playerReference == null && Camera.main != null)
            playerReference = Camera.main.transform;

        if (treeCanvasPrefab == null)
        {
            Debug.LogError($"ObjectTreeAttachment auf '{name}': treeCanvasPrefab fehlt.");
            return;
        }

        GameObject canvasInstance = Instantiate(treeCanvasPrefab);
        treeBuilder = canvasInstance.GetComponentInChildren<TreeMenuBuilder>(true);

        var follow = canvasInstance.GetComponentInChildren<WorldSpaceCanvasFollow>(true);
        if (follow != null)
        {
    
            follow.target = transform;
            follow.offset = canvasOffset;
        }
        else
        {
            Debug.LogWarning($"ObjectTreeAttachment auf '{name}': kein WorldSpaceCanvasFollow " +
                "im treeCanvasPrefab gefunden -- die Canvas bleibt an der im Prefab hinterlegten Position.");
        }
    }

    void Update()
    {
        if (treeBuilder == null || treeBuilder.HasTree) return;

        bool inRange = playerReference != null &&
            Vector3.Distance(transform.position, playerReference.position) <= proximityDistance;

        treeBuilder.SetCreatePromptVisible(inRange);
    }
}