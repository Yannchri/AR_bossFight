using UnityEngine;

public class BossLaserCollision : MonoBehaviour
{
    [SerializeField] float damage = 50;
    [SerializeField] float headDetectionHeight = 0.3f; // Tolérance de hauteur pour la détection de la tête
    [SerializeField] float headDetectionRadius = 0.2f; // Tolérance de distance horizontale pour la détection de la tête
    bool hasHitPlayer = false;
    private OVRCameraRig playerCameraRig;
    private Transform laserOrigin; // Pour obtenir l'origine du laser

    void Start()
    {
        // Trouver la tête du joueur au démarrage
        playerCameraRig = FindFirstObjectByType<OVRCameraRig>();
        
        // Trouver l'origine du laser (typiquement un Transform enfant nommé "LaserOrigin" ou similaire)
        // On cherche dans le parent ou le même objet
        BossLaserAuto laserAuto = GetComponentInParent<BossLaserAuto>();
        if (laserAuto != null && laserAuto.laserOrigin != null)
        {
            laserOrigin = laserAuto.laserOrigin;
        }
        else
        {
            // Fallback : utiliser le parent direct ou cet objet
            laserOrigin = transform.parent != null ? transform.parent : transform;
        }
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

        // NOUVELLE VÉRIFICATION : Le joueur doit être dans l'axe du rayon laser
        // On vérifie que la tête est proche de la ligne entre l'origine du laser et la direction du laser
        if (laserOrigin != null)
        {
            Vector3 laserDirection = transform.forward; // Direction du laser (supposant que le collider pointe dans cette direction)
            Vector3 toHead = headTransform.position - laserOrigin.position;
            
            // Projeter la position de la tête sur l'axe du laser
            float projectionLength = Vector3.Dot(toHead, laserDirection);
            
            // Si la tête est derrière l'origine du laser, elle n'est pas touchée
            if (projectionLength < 0)
            {
                return false;
            }
            
            // Calculer le point le plus proche sur la ligne du laser
            Vector3 closestPointOnLaser = laserOrigin.position + laserDirection * projectionLength;
            
            // Calculer la distance perpendiculaire entre la tête et la ligne du laser
            float distanceFromLaserLine = Vector3.Distance(headTransform.position, closestPointOnLaser);
            
            // Si la tête est trop loin de l'axe du laser, elle n'est pas touchée
            if (distanceFromLaserLine > headDetectionRadius)
            {
                Debug.Log($"Laser miss: Head is {distanceFromLaserLine:F2}m from laser axis (max allowed: {headDetectionRadius:F2}m)");
                return false;
            }
        }

        return true;
    }

    // Optionnel : reset si le laser se désactive
    void OnDisable()
    {
        hasHitPlayer = false;
    }
}
