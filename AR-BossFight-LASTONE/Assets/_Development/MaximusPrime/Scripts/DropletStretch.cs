using UnityEngine;

public class DropletStretch : MonoBehaviour
{
    Rigidbody rb;

    Vector3 baseScale;
    Quaternion baseRotation;

    [Header("Stretch Tuning")]
    public float stretchAmount = 0.15f;   // VERY subtle
    public float stretchSpeed = 8f;        // smoothing

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        baseScale = transform.localScale;
        baseRotation = transform.rotation;
    }

    void LateUpdate()
    {
        if (rb == null || rb.linearVelocity.sqrMagnitude < 0.01f)
        {
            // Return to normal shape smoothly
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                baseScale,
                Time.deltaTime * stretchSpeed
            );
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                baseRotation,
                Time.deltaTime * stretchSpeed
            );
            return;
        }

        Vector3 velocity = rb.linearVelocity;
        float speed = velocity.magnitude;

        // Align droplet with velocity direction
        Quaternion targetRotation = Quaternion.LookRotation(velocity);
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * stretchSpeed
        );

        // Subtle stretch along forward axis
        float stretch = Mathf.Clamp(speed * stretchAmount, 0f, 0.4f);

        Vector3 targetScale = new Vector3(
            baseScale.x - stretch * 0.5f,
            baseScale.y - stretch * 0.5f,
            baseScale.z + stretch
        );

        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * stretchSpeed
        );
    }
}
