using UnityEngine;

public class BossLaserCollision : MonoBehaviour
{
    [SerializeField] float damage = 50;
    [SerializeField] float playerBodyRadius = 0.25f; // Rayon du corps du joueur (environ 25cm)
    [SerializeField] float playerBodyHeight = 1.7f; // Hauteur du corps du joueur (depuis le sol)
    [SerializeField] float playerGroundOffset = 0.1f; // Offset depuis le sol
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

        if (laserOrigin == null)
        {
            Debug.LogWarning("Laser origin not found, cannot check collision accurately");
            return false;
        }

        // Position de la tête du joueur
        Vector3 headPosition = headTransform.position;
        
        // Calculer la position du corps du joueur (centre du cylindre)
        // Le corps part du sol jusqu'à une certaine hauteur
        Vector3 playerGroundPosition = new Vector3(headPosition.x, playerGroundOffset, headPosition.z);
        float actualBodyHeight = headPosition.y - playerGroundOffset; // Hauteur dynamique basée sur la tête
        
        // Si le joueur s'est baissé, la hauteur du corps est réduite
        Vector3 bodyCenter = playerGroundPosition + Vector3.up * (actualBodyHeight * 0.5f);
        
        // Direction et origine du laser
        Vector3 laserDirection = transform.forward.normalized;
        Vector3 laserStart = laserOrigin.position;
        
        // Hauteur du laser
        float laserHeight = laserStart.y;
        
        // Si le laser est au-dessus de la tête ou en dessous du sol, pas de collision
        if (laserHeight > headPosition.y || laserHeight < playerGroundOffset)
        {
            Debug.Log($"Laser evaded by ducking! Laser height: {laserHeight:F2}m, Head height: {headPosition.y:F2}m");
            return false;
        }
        
        // Calculer la distance perpendiculaire entre le corps du joueur et l'axe du laser
        Vector3 toBody = bodyCenter - laserStart;
        float projectionLength = Vector3.Dot(toBody, laserDirection);
        
        // Si le joueur est derrière l'origine du laser, pas de collision
        if (projectionLength < 0)
        {
            return false;
        }
        
        // Point le plus proche sur la ligne du laser
        Vector3 closestPointOnLaser = laserStart + laserDirection * projectionLength;
        
        // Distance horizontale (X, Z) entre le laser et le centre du corps
        Vector3 closestPointFlat = new Vector3(closestPointOnLaser.x, bodyCenter.y, closestPointOnLaser.z);
        Vector3 bodyCenterFlat = new Vector3(bodyCenter.x, bodyCenter.y, bodyCenter.z);
        float horizontalDistance = Vector3.Distance(closestPointFlat, bodyCenterFlat);
        
        // Si le laser est trop loin du corps horizontalement, pas de collision
        if (horizontalDistance > playerBodyRadius)
        {
            Debug.Log($"Laser miss! Distance from body: {horizontalDistance:F2}m (body radius: {playerBodyRadius:F2}m)");
            return false;
        }
        
        Debug.Log($"Laser HIT! Distance: {horizontalDistance:F2}m, Laser height: {laserHeight:F2}m, Head height: {headPosition.y:F2}m");
        return true;
    }

    // Optionnel : reset si le laser se désactive
    void OnDisable()
    {
        hasHitPlayer = false;
    }
}
