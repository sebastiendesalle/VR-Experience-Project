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
        spawnedProp = Instantiate(chosenProp.propPrefab, spawnPos,
                                  playerTransform.rotation);

        spawnedProp.name = "ActiveProp_" + chosenProp.propName;
        GameManager.Instance.activePropObject = spawnedProp;

        Debug.Log("Prop spawned: " + spawnedProp.name);
    }

    void Update()
    {
        // Prop follows player every frame
        if (spawnedProp != null && playerTransform != null)
        {
            spawnedProp.transform.position =
                playerTransform.position + propSpawnOffset;
        }
    }
}