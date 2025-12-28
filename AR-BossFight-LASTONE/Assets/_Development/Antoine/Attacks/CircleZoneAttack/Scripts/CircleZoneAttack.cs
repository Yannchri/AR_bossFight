using System.Collections;
using UnityEngine;

public class CircleZoneAttack : MonoBehaviour
{
    [Header("Réglages")]
    public float tempsAvantExplosion = 2.0f; // Temps pour fuir
    public float dureeExplosion = 0.5f;      // Temps où l'explosion reste affichée
    public float rayonDeDegats = 1.0f;       // Doit correspondre à la taille visuelle
    public int degats = 20;

  

    void Start()
    {
        // Lance la séquence dès que l'objet apparaît
        StartCoroutine(SequenceAttaque());
    }

    IEnumerator SequenceAttaque()
    {
       

        // On attend que le joueur bouge (ou pas)
        yield return new WaitForSeconds(tempsAvantExplosion);

        // PHASE 2 : Boum !
  

        ApplyDamage(); // On vérifie qui est touché

        // On laisse l'explosion visible un court instant
        yield return new WaitForSeconds(dureeExplosion);

        // PHASE 3 : Nettoyage
        Destroy(gameObject);
    }

    void ApplyDamage()
    {
        if (PlayerHealth.Instance == null)
        {
            Debug.LogError("PlayerHealth.Instance NOT FOUND");
            return;
        }

        Vector3 explosionPos = transform.position;
        Vector3 playerPos = PlayerHealth.Instance.GetPlayerPosition();

        // Ignore la hauteur (Y)
        explosionPos.y = 0f;
        playerPos.y = 0f;

        float dist = Vector3.Distance(explosionPos, playerPos);

        if (dist <= rayonDeDegats)
        {
            PlayerHealth.Instance.TakeDamage(degats);
            Debug.Log("Player damaged by explosion (full height)");
        }
    }

    // Juste pour voir la zone de dégâts dans l'éditeur (cercle rouge filaire)
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rayonDeDegats);
    }
}
