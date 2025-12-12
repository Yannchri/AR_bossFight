using UnityEngine;
using System.Linq;

public class HandHUDActivator : MonoBehaviour
{
    [Header("Activation")]
    [Range(0f, 1f)]
    public float threshold = 0.6f;

    [Header("Offset (local wrist space)")]
    public Vector3 offset = new Vector3(0f, 0.05f, 0.02f);

    [Header("References")]
    public Canvas canvas;
    public OVRSkeleton leftHandSkeleton;

    Transform wrist;
    Camera cam;

    void Start()
    {
        cam = Camera.main;
        if (canvas) canvas.enabled = false;
        StartCoroutine(InitWrist());
    }

    System.Collections.IEnumerator InitWrist()
    {
        while (!leftHandSkeleton || !leftHandSkeleton.IsInitialized)
            yield return null;

        wrist = leftHandSkeleton.Bones
            .First(b => b.Id == OVRSkeleton.BoneId.Hand_WristRoot)
            .Transform;
    }

    void LateUpdate()
    {
        if (!wrist || !canvas) return;

        // Paume vers le haut = activation
        Vector3 palmUp = -wrist.up;
        float dot = Vector3.Dot(palmUp, Vector3.up);
        bool show = dot > threshold;

        canvas.enabled = show;
        if (!show) return;

        transform.position = wrist.position + wrist.rotation * offset;

        transform.rotation = Quaternion.LookRotation(
            transform.position - cam.transform.position
        );
    }
}
