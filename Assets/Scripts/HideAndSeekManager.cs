using UnityEngine;
using TMPro;
using Unity.MLAgents;

public class HideAndSeekManager : MonoBehaviour
{
    [Header("Game Settings")]
    public float hidingTime = 30f;
    private float currentTime;
    private bool isHidingPhase = true;

    [Header("References")]
    public TextMeshProUGUI[] timerTexts;
    public GameObject playerHider; // The VR Player

    public DecisionRequester aiBrain;

    void Start()
    {
        // Setup the start of the game
        currentTime = hidingTime;
        isHidingPhase = true;

        // Disable the AI's ability to think/move
        aiBrain.enabled = false;
    }

    void Update()
    {
        if (isHidingPhase)
        {
            currentTime -= Time.deltaTime;

            string timeString = "00:" + Mathf.CeilToInt(currentTime).ToString();

            foreach (TextMeshProUGUI txt in timerTexts)
            {
                if (txt != null)
                {
                    txt.text = timeString;
                }
            }

            if (currentTime <= 0)
            {
                StartSeekingPhase();
            }
        }
    }

    void StartSeekingPhase()
    {
        isHidingPhase = false;

        // Loop through and update all screens to the warning text
        foreach (TextMeshProUGUI txt in timerTexts)
        {
            if (txt != null)
            {
                txt.text = "AI IS SEARCHING!";
                txt.color = Color.red;
            }
        }

        aiBrain.enabled = true;
    }
}