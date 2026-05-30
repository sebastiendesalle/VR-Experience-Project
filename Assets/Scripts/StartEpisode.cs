using Unity.MLAgents;
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

    [Header("Map Settings")]
    // De maximale afstand vanaf het midden van een sectie tot aan de muren van die sectie.
    // Zet deze op een waarde waardoor ze in de kamer blijven en niet in/voorbij de muur spawnen.
    public float maxRoomRadius = 4f;

    [Header("Curriculum Difficulty")]
    public int curriculumLevel
    {
        get
        {
            // Haalt het huidige level (0, 1, 2, 3 of 4) op uit de YAML
            return (int)Academy.Instance.EnvironmentParameters.GetWithDefault("curriculum_level", 0f);
        }
    }

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
        int agentSection = chooseSection();
        int playerSection = chooseSection();

        // Zorg ervoor dat de speler in level 4 NIET in dezelfde kamer spawnt als de agent
        while (playerSection == agentSection)
        {
            playerSection = chooseSection();
        }

        Vector3 agentKamerMidden = SectionMiddleCoordinates[agentSection];
        Vector3 playerKamerMidden = SectionMiddleCoordinates[playerSection];

        // JOUW FASES
        switch (curriculumLevel)
        {
            case 0: // Optie 1: 2m afstand (Baby-stap)
                Agent.transform.localPosition = agentKamerMidden;
                Player.transform.localPosition = GetValidSpawnPosition(Agent.transform.localPosition, 2f);
                break;

            case 1: // Optie 2: 4m afstand, agent in midden van sectie
                Agent.transform.localPosition = agentKamerMidden;
                Player.transform.localPosition = GetValidSpawnPosition(Agent.transform.localPosition, 4f);
                break;

            case 2: // Optie 3: Beide willekeurig in DEZELFDE sectie
                Agent.transform.localPosition = GetValidSpawnPosition(agentKamerMidden, maxRoomRadius);
                Player.transform.localPosition = GetValidSpawnPosition(agentKamerMidden, maxRoomRadius);
                break;

            case 3: // Optie 4: Agent in het midden van de map (gang), speler in willekeurige sectie
                // LET OP: Pas deze Vector3(0, 1, 0) aan als het midden van jouw map ergens anders ligt!
                Agent.transform.localPosition = new Vector3(0, 1, 0);
                Player.transform.localPosition = GetValidSpawnPosition(playerKamerMidden, maxRoomRadius);
                break;

            case 4: // Optie 5: Full Prop Hunt (Agent in kamer A, Speler in kamer B)
                Agent.transform.localPosition = GetValidSpawnPosition(agentKamerMidden, maxRoomRadius);
                Player.transform.localPosition = GetValidSpawnPosition(playerKamerMidden, maxRoomRadius);
                break;
        }

        if (curriculumLevel == 0 || curriculumLevel == 1)
        {
            Vector3 lookTarget = new Vector3(
            Player.transform.position.x,
            Agent.transform.position.y,
            Player.transform.position.z
            );
            Agent.transform.LookAt(lookTarget);
        }
        else
        {
            // Agent random draaien na het spawnen, zodat hij altijd om zich heen moet kijken
            Agent.transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        }
    }

    private Vector3 GetValidSpawnPosition(Vector3 centerPoint, float radius)
    {
        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * radius;

            float randomX = centerPoint.x + randomCircle.x;
            float randomZ = centerPoint.z + randomCircle.y;

            // Klemmen binnen de uiterste buitenmuren van de map
            float buffer = 2f;
            randomX = Mathf.Clamp(randomX, -arenaBoundX + buffer, arenaBoundX - buffer);
            randomZ = Mathf.Clamp(randomZ, -arenaBoundZ + buffer, arenaBoundZ - buffer);

            Vector3 potentialLocalPosition = new Vector3(randomX, 1, randomZ);
            Vector3 worldPosition = transform.TransformPoint(potentialLocalPosition);

            if (!Physics.CheckSphere(worldPosition, playerRadius))
            {
                return potentialLocalPosition;
            }
        }

        Debug.LogWarning("Kamer zat te vol! Spawnt in het exacte midden.");
        return new Vector3(centerPoint.x, 1, centerPoint.z);
    }
}