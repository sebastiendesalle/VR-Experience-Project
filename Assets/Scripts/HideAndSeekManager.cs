using UnityEngine;
using TMPro;
using Unity.MLAgents;

public class HideAndSeekManager : MonoBehaviour
{
    [Header("Game Settings")]
    public float hidingTime = 30f;
    public float seekingTime = 60f;

    [Header("Timer UI - drag all your world timers here")]
    public TextMeshProUGUI[] timerTexts;

    [Header("Phase Text UI - drag a text that shows the phase")]
    public TextMeshProUGUI phaseText;

    [Header("AI")]
    public DecisionRequester aiBrain;
    public GameObject aiAgentObject;
    public Vector3 aiSpawnPosition = new Vector3(0, 1f, 0);

    [Header("VR Control (Lock Mechanisme)")]
    public Behaviour vrMovementComponent;

    // Private
    private float currentTime;
    private bool isHidingPhase = true;
    private bool gameOver = false;

    void Start()
    {
        StartHidingPhase();
    }

    void Update()
    {
        if (gameOver) return;

        // Count down
        currentTime -= Time.deltaTime;
        currentTime = Mathf.Max(0, currentTime);

        // Update all world timers
        UpdateTimerDisplays();

        // Phase switch
        if (currentTime <= 0)
        {
            if (isHidingPhase)
                StartSeekingPhase();
            else
                PlayerWins();
        }
    }

    //  HIDING PHASE

    void StartHidingPhase()
    {
        isHidingPhase = true;
        currentTime = hidingTime;

        if (aiBrain != null)
            aiBrain.enabled = false;

        if (aiAgentObject != null)
        {
            // Teleporteer ver onder de map
            aiAgentObject.transform.position = new Vector3(0, -100f, 0);
            aiAgentObject.SetActive(false); // Zet onzichtbaar en collision uit
        }

        if (vrMovementComponent != null)
        {
            vrMovementComponent.enabled = true;
        }

        if (phaseText != null)
        {
            phaseText.text = "HIDE!";
            phaseText.color = Color.green;
        }

        Debug.Log("Hiding phase started");
    }

    //  SEEKING PHASE

    void StartSeekingPhase()
    {
        isHidingPhase = false;
        currentTime = seekingTime;

        if (vrMovementComponent != null)
        {
            vrMovementComponent.enabled = false; // Zet het lopen via de joystick uit
            Debug.Log("VR Movement Locked! Je kunt alleen nog rondkijken.");
        }

        // --- ZET ALLE COLLIDERS VAN DE PROP AAN ---
        if (GameManager.Instance != null && GameManager.Instance.activePropObject != null)
        {
            Collider[] propColliders = GameManager.Instance.activePropObject.GetComponentsInChildren<Collider>(true);
            foreach (Collider col in propColliders)
            {
                col.enabled = true;
            }
            Debug.Log("Alle colliders op de gekozen prop staan AAN.");
        }

        if (aiAgentObject != null)
        {
            aiAgentObject.transform.position = aiSpawnPosition;
            aiAgentObject.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            Rigidbody rb = aiAgentObject.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            aiAgentObject.SetActive(true);
        }

        if (aiBrain != null)
            aiBrain.enabled = true;

        if (phaseText != null)
        {
            phaseText.text = "AI IS SEARCHING!";
            phaseText.color = Color.red;
        }

        // Turn all timers red
        foreach (TextMeshProUGUI txt in timerTexts)
        {
            if (txt != null)
                txt.color = Color.red;
        }

        Debug.Log("Seeking phase started - AI activated");
    }

    //  END STATES

    public void PlayerFound()
    {
        // Called by the AI script when it reaches the player
        if (gameOver) return;
        gameOver = true;

        if (aiBrain != null)
            aiBrain.enabled = false;

        if (phaseText != null)
        {
            phaseText.text = "YOU WERE FOUND!";
            phaseText.color = Color.red;
        }

        foreach (TextMeshProUGUI txt in timerTexts)
        {
            if (txt != null)
            {
                txt.text = "CAUGHT!";
                txt.color = Color.red;
            }
        }

        Debug.Log("AI found the player - AI wins");
    }

    void PlayerWins()
    {
        if (gameOver) return;
        gameOver = true;

        if (aiBrain != null)
            aiBrain.enabled = false;

        if (phaseText != null)
        {
            phaseText.text = "YOU SURVIVED!";
            phaseText.color = Color.green;
        }

        foreach (TextMeshProUGUI txt in timerTexts)
        {
            if (txt != null)
            {
                txt.text = "YOU WIN!";
                txt.color = Color.green;
            }
        }

        Debug.Log("Timer ran out - Player wins");
    }

    //  TIMER DISPLAY

    void UpdateTimerDisplays()
    {
        int seconds = Mathf.CeilToInt(currentTime);
        string timeString = "00:" + seconds.ToString("00");

        foreach (TextMeshProUGUI txt in timerTexts)
        {
            if (txt != null)
                txt.text = timeString;
        }

        // Flash when under 10 seconds
        if (currentTime <= 10f)
        {
            float flash = Mathf.PingPong(Time.time * 2f, 1f);
            Color flashColor = Color.Lerp(Color.white, Color.red, flash);
            foreach (TextMeshProUGUI txt in timerTexts)
            {
                if (txt != null)
                    txt.color = flashColor;
            }
        }
    }

    //  EDITOR TESTING

#if UNITY_EDITOR
    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 400, 20),
            "S = skip to seeking | F = simulate AI found you");

        if (Input.GetKeyDown(KeyCode.S)) StartSeekingPhase();
        if (Input.GetKeyDown(KeyCode.F)) PlayerFound();
    }
#endif
}