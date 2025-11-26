using UnityEngine;

public class TestDamage : MonoBehaviour
{
    // On a besoin de savoir QUI taper (le script de vie du joueur)
    public PlayerHealth targetPlayer;

    void Update()
    {
        // Quand on appuie sur la BARRE ESPACE du clavier
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("☄️ ATTENTION ! Boule de feu lancée !");

            // On appelle la fonction "Prendre des dégâts" sur le joueur
            if (targetPlayer != null)
            {
                targetPlayer.TakeDamage(10);
            }
        }
    }
}