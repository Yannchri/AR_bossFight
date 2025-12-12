using UnityEngine;

public class Fireball : MonoBehaviour
{
    public int damage = 10;
    
    // Optionnel : Ajouter un effet visuel quand ça touche le bouclier
    // public GameObject hitSparksPrefab; 

    void OnTriggerEnter(Collider other)
    {
        // --- 1. INTERACTION AVEC LE BOUCLIER ---
        if (other.CompareTag("Shield"))
        {
            Debug.Log("<color=cyan>BLOCKED!</color> Le tir a rebondi sur le bouclier.");
            
            // Ici, tu pourras instancier des étincelles plus tard
            // if (hitSparksPrefab != null) Instantiate(hitSparksPrefab, transform.position, Quaternion.identity);

            Destroy(this.gameObject); // On détruit la boule de feu
            return; // IMPORTANT : On arrête la fonction ici pour ne pas blesser le joueur juste après
        }

        // --- 2. INTERACTION AVEC LE JOUEUR ---
        if (other.CompareTag("Player"))
        {
            Debug.Log("<color=red>HIT!</color> Le joueur prend des dégâts.");
            // other.GetComponent<PlayerHealth>().TakeDamage(damage);
        }

        // --- 3. NETTOYAGE ---
        // On détruit la boule si elle touche n'importe quoi (Mur, Sol, Joueur) sauf le Boss lui-même
        if (!other.CompareTag("Boss")) 
        {
            Destroy(this.gameObject);
        }
    }
}