using UnityEngine;

public class RootMotionKiller : MonoBehaviour
{
    [Header("Sleep hier het Hips/Pelvis botje in:")]
    public Transform hipsBone;

    private Vector3 startLocalPos;

    void Start()
    {
        if (hipsBone != null)
        {
            startLocalPos = hipsBone.localPosition;
        }
    }

    void LateUpdate()
    {
        if (hipsBone != null)
        {
            Vector3 current = hipsBone.localPosition;

            hipsBone.localPosition = new Vector3(startLocalPos.x, current.y, startLocalPos.z);
        }
    }
}