using UnityEngine;

public class StartEpisode : MonoBehaviour
{
    public GameObject Player;
    public GameObject Agent;
    public Vector3[] SectionMiddleCoordinates;
    public float SectionOffset;

    public float playerRadius = 0.4f;
    public int maxSpawnAttempts = 30;

    public float arenaBoundX = 20f;
    public float arenaBoundZ = 20f;

    [Header("Curriculum Difficulty")]
    public float spawnRadius = 5f;

    public void Begin()
    {
        this.SetPositions();
    }

    private int chooseSection()
    {
        return Random.Range(0, SectionMiddleCoordinates.Length);
    }

    private void SetPositions()
    {
        // 1. Choose a random section for the agent
        int sectionIndex = this.chooseSection();
        Vector3 chosenSection = SectionMiddleCoordinates[sectionIndex];

        // 2. Spawn the Agent safely near the center of that section
        Agent.transform.localPosition = GetValidSpawnPosition(chosenSection, 2f);

        // Agent spins to a random direction (Training wheels are OFF)
        Agent.transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

        // 3. Spawn the Player relative to the Agent using your Inspector slider
        Player.transform.localPosition = GetValidSpawnPosition(Agent.transform.localPosition, spawnRadius);
    }

    private Vector3 GetValidSpawnPosition(Vector3 centerPoint, float spawnRadius)
    {
        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;

            // 1. Pick a random local coordinate
            float randomX = centerPoint.x + randomCircle.x;
            float randomZ = centerPoint.z + randomCircle.y;

            // 2. CLAMP the coordinates so they cannot escape the physical walls
            float buffer = 2f;
            randomX = Mathf.Clamp(randomX, -arenaBoundX + buffer, arenaBoundX - buffer);
            randomZ = Mathf.Clamp(randomZ, -arenaBoundZ + buffer, arenaBoundZ - buffer);

            Vector3 potentialLocalPosition = new Vector3(randomX, 1, randomZ);

            // 3. CONVERT local to WORLD coordinate for physics checks
            Vector3 worldPosition = transform.TransformPoint(potentialLocalPosition);

            // 4. CHECK physics
            if (!Physics.CheckSphere(worldPosition, playerRadius))
            {
                return potentialLocalPosition;
            }
        }

        Debug.LogWarning("Kamer zat te vol! Spawnt in het exacte midden.");
        return new Vector3(centerPoint.x, 1, centerPoint.z);
    }
}