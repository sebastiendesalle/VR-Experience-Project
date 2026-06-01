using UnityEngine;

public class WaitingRoomIntro : MonoBehaviour
{
    [Header("VR Speler Lock")]
    public Behaviour vrMovementComponent;

    [Header("Sfeer (Het Donkere Effect)")]
    public Light mainLight;
    public float darkIntensity = 0.1f; 
    public float normalIntensity = 1.0f;

    [Header("Het Menu")]
    public GameObject startMenuCanvas;

    [Header("Props")]
    public GameObject props;


    void Start()
    {
        if (vrMovementComponent != null) vrMovementComponent.enabled = false;

        if (mainLight != null) mainLight.intensity = darkIntensity;

        if (startMenuCanvas != null) startMenuCanvas.SetActive(true);

        props.SetActive(false);
    }

    public void StartGameButtonClicked()
    {
        if (vrMovementComponent != null) vrMovementComponent.enabled = true;

        if (mainLight != null) mainLight.intensity = normalIntensity;

        if (startMenuCanvas != null) startMenuCanvas.SetActive(false);

        props.SetActive(true);


        Debug.Log("Waiting room is ontgrendeld. Speler kan nu bewegen!");
    }
}