using UnityEngine;

public class BossLaserCollision : MonoBehaviour
{
    [SerializeField] float damage = 50;
    [SerializeField] float headDetectionHeight = 0.3f; // Tolérance de hauteur pour la détection de la tête
    bool hasHitPlayer = false;
    private OVRCameraRig playerCameraRig;
    [SerializeField] float laserRange = 50f;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] Transform laserOrigin;

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
        if (laserOrigin == null)
            laserOrigin = transform;

        Vector3 direction = laserOrigin.forward;

        Ray ray = new Ray(laserOrigin.position, direction);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, laserRange, playerLayer))
        {
            // Vérifie qu'on touche bien la tête
            if (hit.transform == Camera.main.transform ||
                hit.transform.IsChildOf(Camera.main.transform))
            {
                Debug.DrawLine(ray.origin, hit.point, Color.red);
                return true;
            }
        }

        Debug.DrawLine(ray.origin, ray.origin + direction * laserRange, Color.green);
        return false;
    }

    // Optionnel : reset si le laser se désactive
    void OnDisable()
    {
        hasHitPlayer = false;
    }
}
