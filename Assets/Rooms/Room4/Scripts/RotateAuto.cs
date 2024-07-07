using UnityEngine;

public class RotateAuto : MonoBehaviour
{
    public static bool activate;
    public float spinX, spinY, spinZ;

    void Update()
    {
        transform.Rotate(spinX, spinY, spinZ, Space.Self);
    }
}
