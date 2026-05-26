using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TeleportRing : MonoBehaviour
{
    [Header("Scene To Load")]
    public string mainSceneName = "Main";

    [Header("References")]
    public Renderer ringRenderer;

    [Header("Detection")]
    public float activationRadius = 0.8f;  // How close player needs to be

    [Header("Colors")]
    public Color readyColor = new Color(0f, 1f, 1f, 0.7f);
    public Color notReadyColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
    public Color activatingColor = new Color(1f, 1f, 0f, 0.7f);
    public Color noSelectionColor = new Color(1f, 0f, 0f, 0.7f);

    [Header("Pulse Settings")]
    public float pulseSpeed = 2f;
    public float pulseMin = 0.4f;
    public float pulseMax = 1f;

    private bool isTeleporting = false;
    private Material ringMaterial;
    private Transform xrOrigin;
    private bool wasInsideLastFrame = false;

    void Start()
    {
        // Get material
        if (ringRenderer == null)
            ringRenderer = GetComponent<Renderer>();
        ringMaterial = ringRenderer.material;

        // Find XR Origin by tag
        GameObject xrObj = GameObject.FindGameObjectWithTag("XROrigin");
        if (xrObj != null)
        {
            xrOrigin = xrObj.transform;
            Debug.Log("TeleportRing found XR Origin: " + xrObj.name);
        }
        else
        {
            Debug.Log("TeleportRing could not find XROrigin tag - check tag is set");
        }
    }

    void Update()
    {
        if (isTeleporting) return;
        if (xrOrigin == null) return;

        bool hasProp = GameManager.Instance != null &&
                       GameManager.Instance.chosenProp != null;

        // Check distance between ring center and XR Origin
        float distance = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(xrOrigin.position.x, 0, xrOrigin.position.z)
        );

        bool isInsideRing = distance <= activationRadius;

        // Pulse effect
        float pulse = Mathf.Lerp(pulseMin, pulseMax,
                      (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);

        // Update ring color based on state
        if (isInsideRing && !hasProp)
        {
            // Player inside but no prop chosen - flash red as warning
            ringMaterial.color = noSelectionColor;
            ringMaterial.SetColor("_EmissionColor", noSelectionColor * pulse);

            if (!wasInsideLastFrame)
                Debug.Log("Choose a prop first!");
        }
        else if (hasProp)
        {
            // Prop chosen - ring glows cyan and ready
            ringMaterial.color = readyColor;
            ringMaterial.SetColor("_EmissionColor", readyColor * pulse);
        }
        else
        {
            // No prop, player outside - grey
            ringMaterial.color = notReadyColor;
            ringMaterial.SetColor("_EmissionColor", notReadyColor * pulse);
        }

        // Player just stepped into ring WITH a prop chosen
        if (isInsideRing && hasProp && !wasInsideLastFrame)
        {
            StartCoroutine(TeleportToMainScene());
        }

        wasInsideLastFrame = isInsideRing;
    }

    IEnumerator TeleportToMainScene()
    {
        if (isTeleporting) yield break;
        isTeleporting = true;

        Debug.Log("Teleporting as: " + GameManager.Instance.chosenProp.propName);

        // Flash yellow
        ringMaterial.color = activatingColor;
        ringMaterial.SetColor("_EmissionColor", activatingColor * 3f);

        yield return new WaitForSeconds(0.8f);

        SceneManager.LoadScene(mainSceneName);
    }

    // Draw the detection radius in the editor so you can see it
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, activationRadius);
    }
}