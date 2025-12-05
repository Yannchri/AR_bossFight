using UnityEngine;

public class HandHUDActivator : MonoBehaviour
{
    [Header("Activation")]
    [Range(0f, 1f)]
    public float threshold = 0.6f;

    [Header("Position")]
    public float palmOffset = 0.09f;

    [Header("References")]
    public Canvas canvas;

    private Transform hand;
    private Camera cam;

    void Start()
    {
        hand = transform.parent;
        cam = FindObjectOfType<Camera>();

        if (canvas != null)
            canvas.enabled = false;
    }

    void Update()
    {
        if (!hand || !canvas) return;

        // Normale de la paume (Meta Quest)
        Vector3 palmNormal = -hand.up;

        float dot = Vector3.Dot(palmNormal, Vector3.up);
        bool show = dot > threshold;

        canvas.enabled = show;

        if (!show) return;

        // POSITION : devant la paume
        transform.position = hand.position + palmNormal * palmOffset;
    }

    void LateUpdate()
    {
        if (!canvas.enabled || cam == null) return;

        // Toujours face au casque (ignore rotation main)
        transform.rotation = Quaternion.LookRotation(
            transform.position - cam.transform.position,
            Vector3.up
        );
    }
}
