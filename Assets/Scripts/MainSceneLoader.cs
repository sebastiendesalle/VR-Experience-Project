using UnityEngine;

public class MainSceneLoader : MonoBehaviour
{
    [Header("References")]
    public GameObject playerVisual;
    public Transform playerTransform;

    [Header("Prop Offset")]
    public Vector3 propSpawnOffset = new Vector3(0, 0, 0);

    private GameObject spawnedProp;

    void Start()
    {
        // Diagnosis logs so we know exactly whats happening
        if (GameManager.Instance == null)
        {
            Debug.Log("FAIL - No GameManager found in main scene");
            return;
        }

        if (GameManager.Instance.chosenProp == null)
        {
            Debug.Log("FAIL - chosenProp is NULL - player never selected a prop");
            return;
        }

        Debug.Log("SUCCESS - Spawning as: " +
                  GameManager.Instance.chosenProp.propName);
        SpawnChosenProp();
    }

    void SpawnChosenProp()
    {
        PropData chosenProp = GameManager.Instance.chosenProp;

        // Hide the player sphere
        if (playerVisual != null)
            playerVisual.SetActive(false);
        else
            Debug.Log("WARNING - playerVisual is not assigned");

        // Spawn the prop at player position
        Vector3 spawnPos = playerTransform.position + propSpawnOffset;
        spawnedProp = Instantiate(chosenProp.propPrefab, spawnPos, Quaternion.identity);
        StripPhysicsFromProp(spawnedProp);

        spawnedProp.name = "ActiveProp_" + chosenProp.propName;
        GameManager.Instance.activePropObject = spawnedProp;
        spawnedProp.tag = "Player";

        Debug.Log("Prop spawned: " + spawnedProp.name);
    }

    private void StripPhysicsFromProp(GameObject prop)
    {
        Rigidbody rb = prop.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        int propLayerIndex = LayerMask.NameToLayer("Ignore Raycast");

        if (propLayerIndex == -1)
        {
            Debug.LogError("Let op! Je hebt de layer 'ignore Raycast' nog niet aangemaakt in Unity!");
        }

        prop.layer = propLayerIndex;

        foreach (Collider col in prop.GetComponentsInChildren<Collider>())
        {
            col.enabled = true;
            col.isTrigger = false;
            col.gameObject.layer = propLayerIndex;
        }
    }

    void LateUpdate()
    {
        if (spawnedProp != null)
            spawnedProp.transform.position = playerTransform.position + propSpawnOffset;
    }
}