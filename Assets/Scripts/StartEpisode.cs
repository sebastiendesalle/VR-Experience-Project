using UnityEngine;

public class StartEpisode : MonoBehaviour
{
    public GameObject Player;
    public GameObject Agent;
    public Vector3[] SectionMiddleCoordinates;
    public float SectionOffset;

    public float playerRadius = 0.4f;
    public int maxSpawnAttempts = 30;
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
        // Agent
        Agent.transform.localPosition = new Vector3(0, 1, 0);
        Agent.transform.localRotation = Quaternion.identity;

        //Speler
        int sectionIndex = this.chooseSection();
        Vector3 chosenSection = SectionMiddleCoordinates[sectionIndex];

        Vector3 safePosition = GetValidSpawnPosition(chosenSection);
        this.Player.transform.localPosition = safePosition;
    }

    private Vector3 GetValidSpawnPosition(Vector3 sectionMiddle)
    {
        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            float playerX = Random.Range(sectionMiddle.x - SectionOffset, sectionMiddle.x + SectionOffset);
            float playerZ = Random.Range(sectionMiddle.z - SectionOffset, sectionMiddle.z + SectionOffset);

            Vector3 potentialPosition = new Vector3(playerX, 1, playerZ);

            if (!Physics.CheckSphere(potentialPosition, playerRadius))
            {
                return potentialPosition;
            }
        }
        Debug.LogWarning("Kamer zat te vol! Speler spawnt in het midden van de sectie.");
        return new Vector3(sectionMiddle.x, 1, sectionMiddle.z);
    }
}
