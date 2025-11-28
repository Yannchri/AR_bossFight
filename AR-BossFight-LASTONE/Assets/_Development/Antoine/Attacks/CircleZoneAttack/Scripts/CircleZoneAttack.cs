using System.Collections;
using UnityEngine;

public class CircleZoneAttack : MonoBehaviour
{
    [Header("Réglages")]
    public float tempsAvantExplosion = 2.0f; // Temps pour fuir
    public float dureeExplosion = 0.5f;      // Temps où l'explosion reste affichée
    public float rayonDeDegats = 1.0f;       // Doit correspondre à la taille visuelle
    public int degats = 20;

    [Header("Visuels")]
    public GameObject warningVisual;   // Glisse le WarningCircle ici
    public GameObject explosionVisual; // Glisse le ExplosionEffect ici

    void Start()
    {
        // Lance la séquence dès que l'objet apparaît
        StartCoroutine(SequenceAttaque());
    }

    IEnumerator SequenceAttaque()
    {
        // PHASE 1 : Prévention
        warningVisual.SetActive(true);
        explosionVisual.SetActive(false);

        // On attend que le joueur bouge (ou pas)
        yield return new WaitForSeconds(tempsAvantExplosion);

        // PHASE 2 : Boum !
        warningVisual.SetActive(false);
        explosionVisual.SetActive(true);

        ApplyDamage(); // On vérifie qui est touché

        // On laisse l'explosion visible un court instant
        yield return new WaitForSeconds(dureeExplosion);

        // PHASE 3 : Nettoyage
        Destroy(gameObject);
    }

    void ApplyDamage()
    {
        // On détecte tout ce qui est dans la sphère au moment T
        Collider[] objetsTouches = Physics.OverlapSphere(transform.position, rayonDeDegats);

        foreach (Collider col in objetsTouches)
        {
            if (col.CompareTag("Player"))
            {
                Debug.Log("<color=red>AIE ! Le joueur a pris " + degats + " dégâts !</color>");
                // Plus tard : col.GetComponent<PlayerHealth>().TakeDamage(degats);
            }
        }
    }

    // Juste pour voir la zone de dégâts dans l'éditeur (cercle rouge filaire)
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rayonDeDegats);
    }
}
