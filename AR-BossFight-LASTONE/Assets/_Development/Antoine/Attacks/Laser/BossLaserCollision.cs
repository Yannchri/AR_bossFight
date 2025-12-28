using UnityEngine;

public class BossLaserCollision : MonoBehaviour
{
    [SerializeField] float damage = 50;
    bool hasHitPlayer = false;

    void OnTriggerEnter(Collider other)
    {
        if (hasHitPlayer)
            return;

        if (PlayerHealth.Instance == null)
        {
            Debug.LogError("PlayerHealth.Instance NOT FOUND");
            return;
        }

        // On vérifie que c’est bien le joueur (via la position, pas le collider)
        // Le laser est déjà un trigger spatial, donc on applique directement
        hasHitPlayer = true;

        PlayerHealth.Instance.TakeDamage(damage);
        Debug.Log("Player damaged by LASER");
    }

    // Optionnel : reset si le laser se désactive
    void OnDisable()
    {
        hasHitPlayer = false;
    }
}
