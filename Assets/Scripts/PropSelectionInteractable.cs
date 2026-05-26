using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class PropSelectionInteractable : MonoBehaviour
{
    [Header("Which prop does this pedestal represent?")]
    public PropData propData;

    [Header("Visual Feedback")]
    public Renderer propRenderer;
    public Color normalColor = Color.gray;
    public Color hoverColor = Color.yellow;
    public Color selectedColor = Color.green;

    [Header("Testing")]
    public KeyCode testKey = KeyCode.K;  // Change this per prop in Inspector

    private XRSimpleInteractable interactable;
    private bool isSelected = false;

    void Awake()
    {
        interactable = gameObject.AddComponent<XRSimpleInteractable>();

        if (GetComponent<Collider>() == null)
            gameObject.AddComponent<BoxCollider>();

        if (propRenderer == null)
            propRenderer = GetComponent<Renderer>();
    }

    void OnEnable()
    {
        interactable.hoverEntered.AddListener(OnHoverEnter);
        interactable.hoverExited.AddListener(OnHoverExit);
        interactable.selectEntered.AddListener(OnSelected);
    }

    void OnDisable()
    {
        interactable.hoverEntered.RemoveListener(OnHoverEnter);
        interactable.hoverExited.RemoveListener(OnHoverExit);
        interactable.selectEntered.RemoveListener(OnSelected);
    }

    void OnHoverEnter(HoverEnterEventArgs args)
    {
        if (!isSelected)
            propRenderer.material.color = hoverColor;
    }

    void OnHoverExit(HoverExitEventArgs args)
    {
        if (!isSelected)
            propRenderer.material.color = normalColor;
    }

    void OnSelected(SelectEnterEventArgs args)
    {
        SelectThisProp();
    }

    void SelectThisProp()
    {
        // Find the transformer on the XR Origin
        PlayerPropTransformer transformer =
            FindObjectOfType<PlayerPropTransformer>();

        if (transformer == null)
        {
            Debug.Log("No PlayerPropTransformer found in scene");
            return;
        }

        // Transform the player immediately
        transformer.TransformIntoProp(propData);

        // Turn this cube green
        propRenderer.material.color = selectedColor;
        isSelected = true;

        // Reset all other cubes to grey
        PropSelectionInteractable[] allProps =
            FindObjectsOfType<PropSelectionInteractable>();
        foreach (var prop in allProps)
        {
            if (prop != this)
                prop.Deselect();
        }
    }

    public void Deselect()
    {
        isSelected = false;
        propRenderer.material.color = normalColor;
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(testKey))
        {
            Debug.Log("Test key pressed - forcing selection of: " + propData.propName);
            SelectThisProp();
        }
#endif
    }
}