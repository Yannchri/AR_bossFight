using UnityEngine;

public class BossLaserCollision : MonoBehaviour
{
    // Si le joueur touche le rayon, aïe !
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("LE JOUEUR EST TOUCHÉ PAR LE LASER !");
            // Plus tard : PlayerHealth.TakeDamage(50);
        }
    }
}