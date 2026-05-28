using UnityEngine;

public class PlayerPropTransformer : MonoBehaviour
{
    [Header("References")]
    public GameObject playerVisual;        // The Player sphere
    public Transform playerTransform;      // XR Origin transform
    public Transform cameraTransform;      // Main Camera inside XR Origin

    [Header("Prop Offset")]
    public Vector3 propSpawnOffset = new Vector3(0, 0, 0);

    // Currently spawned prop
    private GameObject spawnedProp;

    void LateUpdate()
    {
        if (spawnedProp != null)
            spawnedProp.transform.position = playerTransform.position + propSpawnOffset;
    }

    // Called by PropSelectionInteractable when player confirms choice
    public void TransformIntoProp(PropData propData)
    {
        if (propData == null || propData.propPrefab == null) return;

        if (spawnedProp != null)
        {
            Destroy(spawnedProp);
            spawnedProp = null;
        }

        if (playerVisual != null)
            playerVisual.SetActive(false);

        Vector3 spawnPosition = playerTransform.position + propSpawnOffset;
        spawnedProp = Instantiate(propData.propPrefab, spawnPosition, Quaternion.identity); // <-- Quaternion.identity, not playerTransform.rotation

        // Strip all physics from the prop — it's visual only
        StripPhysicsFromProp(spawnedProp);

        spawnedProp.name = "ActiveProp_" + propData.propName;
        GameManager.Instance.chosenProp = propData;
        GameManager.Instance.isTransformed = true;
        GameManager.Instance.activePropObject = spawnedProp;

        Debug.Log("Transformed into: " + propData.propName);
    }

    private void StripPhysicsFromProp(GameObject prop)
    {
        // Disable Rigidbody — this is what causes the physics conflict
        Rigidbody rb = prop.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Disable all colliders — the player's own collider handles wall collision
        foreach (Collider col in prop.GetComponentsInChildren<Collider>())
            col.enabled = false;
    }

    // Call this if player wants to pick a different prop
    public void Untransform()
    {
        if (spawnedProp != null)
        {
            Destroy(spawnedProp);
            spawnedProp = null;
        }

        if (playerVisual != null)
            playerVisual.SetActive(true);

        GameManager.Instance.isTransformed = false;
        GameManager.Instance.activePropObject = null;

        Debug.Log("Reverted to player");
    }
}