using UnityEngine;

public class Fireball : MonoBehaviour
{
    public int damage = 10;

    // Se déclenche quand la boule touche quelque chose
    void OnTriggerEnter(Collider other)
    {
        // On vérifie si c'est le joueur (assure-toi que ton joueur a le Tag "Player")
        if (other.CompareTag("Player"))
        {
            Debug.Log("BOUM ! Le joueur est touché !");
            
            // Ici, tu appelleras plus tard le script de vie du joueur
            // ex: other.GetComponent<PlayerHealth>().TakeDamage(damage);
        }

        // On détruit la boule de feu au contact (sauf si c'est le boss lui-même pour éviter qu'il se tire dessus)
        if (!other.CompareTag("Boss")) 
        {
            Destroy(this.gameObject);
        }
    }
}