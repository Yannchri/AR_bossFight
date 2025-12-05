using UnityEngine;

public class MetaShieldController : MonoBehaviour
{
    [Header("Réglages Visuels")]
    public GameObject shieldVisual;

    [Header("Sources d'Input")]
    public OVRHand handTracking;
    public OVRInput.Controller controllerType = OVRInput.Controller.LTouch;

    [Header("Réglages Sensibilité")]
    [Range(0f, 1f)] public float pinchThresholdOn = 0.5f; // Active à 50% de fermeture
    [Range(0f, 1f)] public float pinchThresholdOff = 0.4f; // Désactive si on relâche en dessous de 40%
    // Avoir deux seuils différents évite le clignotement (Hysteresis)

    [Header("Debug")]
    public bool useKeyboardDebug = true;
    public KeyCode debugKey = KeyCode.S;

    private bool isShieldActive = false;

    void Start()
    {
        if (shieldVisual != null) shieldVisual.SetActive(false);
    }

    void Update()
    {
        bool shouldActivate = false;

        // --- 1. CLAVIER ---
        if (useKeyboardDebug && Input.GetKey(debugKey)) shouldActivate = true;

        // --- 2. MANETTE (Grip) ---
        // On garde le seuil classique pour la manette
        if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, controllerType) > 0.8f)
            shouldActivate = true;

        // --- 3. MAIN NUE (Le "Grip" Tolérant) ---
        if (handTracking != null && handTracking.IsTracked)
        {
            // On récupère la force de pincement (0 à 1) pour le Majeur et l'Annulaire
            float middleStrength = handTracking.GetFingerPinchStrength(OVRHand.HandFinger.Middle);
            float ringStrength = handTracking.GetFingerPinchStrength(OVRHand.HandFinger.Ring);

            // On prend le plus fort des deux (comme ça, pas besoin d'être précis)
            float maxStrength = Mathf.Max(middleStrength, ringStrength);

            // LOGIQUE HYSTERESIS (Anti-clignotement)
            if (isShieldActive)
            {
                // Si déjà actif, on reste actif tant qu'on ne relâche pas franchement
                if (maxStrength > pinchThresholdOff) shouldActivate = true;
            }
            else
            {
                // Si inactif, on demande un effort pour activer
                if (maxStrength > pinchThresholdOn) shouldActivate = true;
            }
        }

        // APPLICATION
        if (shouldActivate != isShieldActive)
        {
            isShieldActive = shouldActivate;
            UpdateShieldState();
        }
    }

    void UpdateShieldState()
    {
        if (shieldVisual != null) shieldVisual.SetActive(isShieldActive);
    }

    public bool IsActive() => isShieldActive;
}