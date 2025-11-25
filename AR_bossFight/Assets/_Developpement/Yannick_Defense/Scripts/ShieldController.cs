using UnityEngine;

public class ShieldController : MonoBehaviour
{
    public GameObject shieldVisual; // Glisse l'objet Cylinder ici
    public bool isShieldActive = false;

    void Update()
    {
        // --- LOGIQUE DE TEST (Simulateur / Clavier) ---
        // Appuie sur 'Shift gauche' (simulation main gauche) + 'G' (Grip) ou juste une touche clavier simple
        // Pour faire simple au début : Appuie sur la touche 'S' pour Shield
        if (Input.GetKey(KeyCode.S))
        {
            ActivateShield(true);
        }
        else
        {
            ActivateShield(false);
        }
    }

    void ActivateShield(bool active)
    {
        isShieldActive = active;
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(active);
        }
    }

    // Cette fonction sera appelée par le Boss quand il attaque
    // Elle renvoie TRUE si l'attaque est bloquée
    public bool TryBlockAttack()
    {
        if (isShieldActive)
        {
            Debug.Log("🛡️ Attaque bloquée !");
            // Ici on pourra ajouter un son de blocage
            return true;
        }
        return false;
    }
}