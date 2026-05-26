using UnityEngine;

[CreateAssetMenu(fileName = "NewProp", menuName = "PropHunt/PropData")]
public class PropData : ScriptableObject
{
    [Header("Identify")]
    // Name of the prop "Chair", "Crate", etc.
    public string propName;
    // The 3D model prefab
    public GameObject propPrefab;

    [Header("Player Fit")]
    // Size of the box collider when transformed
    public Vector3 colliderSize;
    // Offset of collider from player feet
    public Vector3 colliderCenter;
    // Where VR camera sits inside the prop
    public Vector3 cameraOffset;

    [Header("Visual")]
    // For the selection UI 
    public Sprite previewImage;
}
