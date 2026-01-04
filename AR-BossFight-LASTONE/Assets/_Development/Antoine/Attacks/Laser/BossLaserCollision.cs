using UnityEngine;

public class BossLaserCollision : MonoBehaviour
{
    [SerializeField] float damage = 50;
    [SerializeField] float headDetectionHeight = 0.3f; // Tolérance de hauteur pour la détection de la tête
    bool hasHitPlayer = false;
    private OVRCameraRig playerCameraRig;

    void Start()
    {
        // Trouver la tête du joueur au démarrage
        playerCameraRig = FindFirstObjectByType<OVRCameraRig>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasHitPlayer)
            return;

        if (PlayerHealth.Instance == null)
        {
            Debug.LogError("PlayerHealth.Instance NOT FOUND");
            return;
        }

        // Vérifier que le laser touche réellement la tête du joueur
        if (!IsLaserHittingHead())
        {
            return;
        }

        hasHitPlayer = true;
        PlayerHealth.Instance.TakeDamage(damage);
        Debug.Log("Player damaged by LASER");
    }

    bool IsLaserHittingHead()
    {
        // Si on n'a pas trouvé la tête, chercher Camera.main comme fallback
        Transform headTransform = playerCameraRig != null ? playerCameraRig.centerEyeAnchor : Camera.main?.transform;

        if (headTransform == null)
        {
            Debug.LogWarning("Could not find player head for laser detection");
            return false;
        }

        // Vérifier que le laser est à la bonne hauteur (hauteur de la tête)
        float laserHeight = transform.position.y;
        float headHeight = headTransform.position.y;
        
        // Le laser doit être approximativement à la hauteur de la tête
        if (Mathf.Abs(laserHeight - headHeight) > headDetectionHeight)
        {
            return false;
        }

        return true;
    }

    // Optionnel : reset si le laser se désactive
    void OnDisable()
    {
        hasHitPlayer = false;
    }
}
