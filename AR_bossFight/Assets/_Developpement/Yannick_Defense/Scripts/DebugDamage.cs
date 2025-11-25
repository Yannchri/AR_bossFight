using UnityEngine;

public class DebugDamage : MonoBehaviour
{
    public PlayerStats playerStats; // Glisse le OVRCameraRig ici
    public ShieldController shieldController; // Glisse le LeftHandAnchor ici

    void Update()
    {
        // Appuie sur Espace pour simuler une boule de feu reçue
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SimulateAttack();
        }
    }

    void SimulateAttack()
    {
        // On demande au bouclier s'il bloque
        if (shieldController != null && shieldController.TryBlockAttack())
        {
            Debug.Log("Tir ennemi bloqué par le bouclier !");
        }
        else
        {
            Debug.Log("Aïe ! Dégâts reçus.");
            playerStats.TakeDamage(10);
        }
    }
}