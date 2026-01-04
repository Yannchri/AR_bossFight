using UnityEngine;

public class BodySync : MonoBehaviour
{
    [Header("Références")]
    public Transform cameraTransform; // Glisse le "CenterEyeAnchor" ici
    private CharacterController _controller;

    void Start()
    {
        _controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (cameraTransform == null || _controller == null) return;

        // On aligne le centre de la capsule sur la position horizontale de la caméra
        // On garde la hauteur (y) locale à 0 pour que le pied reste au sol
        Vector3 newCenter = cameraTransform.localPosition;
        newCenter.y = _controller.height / 2; // Centre vertical de la capsule

        _controller.center = newCenter;
    }
}