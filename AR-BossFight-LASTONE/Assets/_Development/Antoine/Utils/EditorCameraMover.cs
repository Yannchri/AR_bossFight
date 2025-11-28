using UnityEngine;
using UnityEngine.InputSystem;

public class EditorComeraMover : MonoBehaviour
{
public float moveSpeed = 5.0f;
    public float sensitivity = 0.5f; // Sensibilité ajustée pour le nouveau système

    void Update()
    {
#if UNITY_EDITOR
        // Vérifier si la souris et le clavier existent (évite les crashs si manette connectée)
        if (Mouse.current == null || Keyboard.current == null) return;

        // --- 1. Rotation avec la souris (Clic Droit) ---
        if (Mouse.current.rightButton.isPressed)
        {
            // On lit le delta (le déplacement) de la souris
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            
            float rotateHorizontal = mouseDelta.x * sensitivity;
            float rotateVertical = mouseDelta.y * sensitivity;

            transform.Rotate(0, rotateHorizontal, 0, Space.World);
            transform.Rotate(-rotateVertical, 0, 0, Space.Self);
        }

        // --- 2. Déplacement Clavier (ZQSD / WASD) ---
        float moveX = 0;
        float moveZ = 0;

        // Lecture manuelle des touches
        if (Keyboard.current.wKey.isPressed || Keyboard.current.zKey.isPressed) moveZ = 1;
        if (Keyboard.current.sKey.isPressed) moveZ = -1;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.qKey.isPressed) moveX = -1;
        if (Keyboard.current.dKey.isPressed) moveX = 1;

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        
        // On garde le y à 0 pour rester au sol
        move.y = 0; 

        transform.position += move * moveSpeed * Time.deltaTime;
#endif
    }
}