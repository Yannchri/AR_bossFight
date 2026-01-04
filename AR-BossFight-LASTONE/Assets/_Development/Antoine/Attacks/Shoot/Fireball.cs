using UnityEngine;

public class Fireball : MonoBehaviour
{
    public int damage = 10;
    public float lifeTime = 10f; // On définit la durée de vie ici

    void Start()
    {
        // Cette ligne programme la destruction de l'objet 10 secondes après son apparition
        // "gameObject" est l'objet lui-même, "lifeTime" est le délai
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // --- 1. BOUCLIER ---
        if (other.CompareTag("Shield"))
        {
            Debug.Log("<color=cyan>BLOCKED!</color>");
            Destroy(gameObject); // On détruit tout de suite si c'est bloqué
            return;
        }

        // --- 2. JOUEUR ---
        if (other.CompareTag("MainCamera"))
        { 
            if (PlayerHealth.Instance != null)
            {
                PlayerHealth.Instance.TakeDamage(damage);
                Debug.Log("Player damaged!");
            }
            Destroy(gameObject); // On détruit tout de suite si le joueur est touché
            return;
        }
    }
}
