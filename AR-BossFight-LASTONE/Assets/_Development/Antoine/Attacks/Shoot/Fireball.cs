using UnityEngine;

public class Fireball : MonoBehaviour
{
    public int damage = 10;

    void OnTriggerEnter(Collider other)
    {
        // --- 1. BOUCLIER ---
        if (other.CompareTag("Shield"))
        {
            Debug.Log("<color=cyan>BLOCKED!</color> Fireball blocked by shield.");
            Destroy(gameObject);
            return;
        }

        // --- 2. JOUEUR ---
        if (other.CompareTag("PlayerBody"))
        { 
            PlayerHealth.Instance.TakeDamage(damage);

            Debug.Log("Player damaged by FIREBALL");
        }
        else
        {
            Debug.LogError("PlayerHealth.Instance NOT FOUND");
        }

        // --- 3. NETTOYAGE ---
        Destroy(gameObject);
    }
}
