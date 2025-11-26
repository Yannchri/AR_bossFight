using UnityEngine;
using UnityEngine.InputSystem; // Indispensable pour parler au nouveau système

public class ShieldController : MonoBehaviour
{
    [Header("Input Configuration")]
    // On réutilise la même propriété que dans ton script d'animation
    public InputActionProperty gripValue;

    [Header("Visuals")]
    public GameObject shieldVisual; // Ton objet bouclier
    public float activationThreshold = 0.8f; // À quel point il faut appuyer (0 à 1)

    private bool isShieldActive = false;

    void Update()
    {
        // 1. Lire la valeur du Grip (comme dans ton script AnimateHand)
        float grip = gripValue.action.ReadValue<float>();

        // 2. Vérifier si on dépasse le seuil
        bool shouldBeActive = grip > activationThreshold;

        // 3. Appliquer l'état seulement si ça change (pour éviter de spammer)
        if (shouldBeActive != isShieldActive)
        {
            isShieldActive = shouldBeActive;
            UpdateShieldState();
        }
    }

    void UpdateShieldState()
    {
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(isShieldActive);

            if (isShieldActive)
            {
                // Ici on mettra un son "Woshhh" d'activation plus tard
                Debug.Log("🛡️ BOUCLIER ACTIVÉ !");
            }
        }
    }

    // Cette fonction sera appelée par le Boss pour savoir s'il t'a touché
    public bool IsBlocking()
    {
        return isShieldActive;
    }
}