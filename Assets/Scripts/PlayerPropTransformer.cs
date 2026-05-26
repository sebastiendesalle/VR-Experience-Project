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

    void Update()
    {
        // Prop follows player every frame
        if (spawnedProp != null)
        {
            spawnedProp.transform.position =
                playerTransform.position + propSpawnOffset;
        }
    }

    // Called by PropSelectionInteractable when player confirms choice
    public void TransformIntoProp(PropData propData)
    {
        if (propData == null || propData.propPrefab == null)
        {
            Debug.Log("PropData or prefab is missing");
            return;
        }

        // If already transformed into something, remove it first
        if (spawnedProp != null)
        {
            Destroy(spawnedProp);
            spawnedProp = null;
        }

        // Hide the player sphere
        if (playerVisual != null)
            playerVisual.SetActive(false);

        // Spawn the prop at player position
        Vector3 spawnPosition = playerTransform.position + propSpawnOffset;
        spawnedProp = Instantiate(propData.propPrefab, spawnPosition,
                                  playerTransform.rotation);

        spawnedProp.name = "ActiveProp_" + propData.propName;

        // Save everything to GameManager
        GameManager.Instance.chosenProp = propData;
        GameManager.Instance.isTransformed = true;
        GameManager.Instance.activePropObject = spawnedProp;

        Debug.Log("Transformed into: " + propData.propName);
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