using UnityEngine;

public class MetaShieldController : MonoBehaviour
{
    [Header("Réglages")]
    public GameObject shieldVisual; // L'objet 3D du bouclier
    public float gripThreshold = 0.8f; // Seuil de pression (0 à 1)

    [Header("Debug PC")]
    public bool useKeyboardDebug = true; // Coche ça pour tester sans casque
    public KeyCode debugKey = KeyCode.S;

    private bool isShieldActive = false;

    void Start()
    {
        // Au démarrage, on cache le bouclier
        if (shieldVisual != null) shieldVisual.SetActive(false);
    }

    void Update()
    {
        bool inputDetected = false;

        // 1. DÉTECTION META (Pour le casque)
        // On lit le "Hand Trigger" (Grip) de la main GAUCHE (LTouch)
        float gripValue = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.LTouch);

        if (gripValue > gripThreshold)
        {
            inputDetected = true;
        }

        // 2. DÉTECTION CLAVIER (Pour toi maintenant)
        if (useKeyboardDebug && Input.GetKey(debugKey))
        {
            inputDetected = true;
        }

        // 3. LOGIQUE D'ACTIVATION
        if (inputDetected != isShieldActive)
        {
            isShieldActive = inputDetected;
            UpdateShieldState();
            
        }
    }

    void UpdateShieldState()
    {
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(isShieldActive);
        }
    }

    // Fonction que le Boss appellera pour savoir s'il touche le bouclier
    public bool IsActive()
    {
        return isShieldActive;
    }
}