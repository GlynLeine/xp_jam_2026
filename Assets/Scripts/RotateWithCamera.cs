using UnityEngine;

public class RotateWithCamera : MonoBehaviour
{
    public Transform cameraTransform;

    // Update is called once per frame
    void Update()
    {
        transform.forward = cameraTransform.forward;
    }
}
