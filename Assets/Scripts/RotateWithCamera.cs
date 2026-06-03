using UnityEngine;

public class RotateWithCamera : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        transform.forward = Camera.main.transform.forward;
    }
}
