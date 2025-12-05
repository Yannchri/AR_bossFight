using UnityEngine;

public class HandHUDAnchor : MonoBehaviour
{
    Transform palm;
    public float offset = 0.06f;

    public void Init(Transform palmAnchor)
    {
        palm = palmAnchor;
    }

    void LateUpdate()
    {
        if (!palm) return;

        Vector3 normal = -palm.up;
        transform.position = palm.position + normal * offset;

        Camera cam = Camera.main;
        Vector3 dir = transform.position - cam.transform.position;
        transform.rotation = Quaternion.LookRotation(dir);
    }
}
