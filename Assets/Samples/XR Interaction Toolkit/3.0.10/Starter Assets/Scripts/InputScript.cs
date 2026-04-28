using UnityEngine;
using UnityEngine.InputSystem;

public class InputScript : MonoBehaviour
{
    [SerializeField]
    public InputActionReference thumbStick; // Referentie naar je joystick actie

    void Start()
    {
        // Luisteren of de joystick bewogen wordt
        thumbStick.action.performed += ThumbStick;
    }

    private void ThumbStick(InputAction.CallbackContext obj)
    {
        // Lees de X en Y waarden uit als een Vector2
        Vector2 val = obj.ReadValue<Vector2>();

        // Print de waarden (bijv. X: 0.5, Y: -1) in de console
        print(val);
    }
}