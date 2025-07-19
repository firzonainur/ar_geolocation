using UnityEngine;

public class CanvasFollowCamera : MonoBehaviour
{
    public Camera targetCamera;
    public Vector3 offset = new Vector3(0, 0, 2f);
    public float positionSmoothTime = 0.2f;
    public float rotationSmoothTime = 0.2f;

    private Vector3 velocity = Vector3.zero;
    private Quaternion targetRotation;

    private bool isFrozen = false;

    void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (targetCamera == null || isFrozen) return;

        Vector3 desiredPosition = targetCamera.transform.position +
                                  targetCamera.transform.forward * offset.z +
                                  targetCamera.transform.up * offset.y +
                                  targetCamera.transform.right * offset.x;

        Quaternion desiredRotation = Quaternion.LookRotation(desiredPosition - targetCamera.transform.position);

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, positionSmoothTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime / rotationSmoothTime);
    }

    // Method untuk digunakan Toggle
    public void SetFreezeState(bool freeze)
    {
        isFrozen = freeze;
    }
}
