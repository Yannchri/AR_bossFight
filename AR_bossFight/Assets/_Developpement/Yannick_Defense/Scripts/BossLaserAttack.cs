using UnityEngine;
using System.Collections;

public class BossLaserAttack : MonoBehaviour
{
    [Header("Réglages")]
    public Transform targetPlayer; // Glisse le XR Origin ici
    public float damage = 20f;
    public float laserDuration = 0.2f; // Temps d'affichage du laser
    public float range = 50f;

    [Header("Composants")]
    public LineRenderer laserLine;

    void Update()
    {
        // Le Boss regarde toujours le joueur (flippant !)
        if (targetPlayer != null)
        {
            transform.LookAt(targetPlayer);
        }

        // TEST : Appuie sur ENTRÉE pour tirer
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ShootLaser();
        }
    }

    void ShootLaser()
    {
        // 1. On allume le visuel
        StartCoroutine(FireEffect());

        // 2. On prépare le Raycast (Un rayon invisible qui part tout droit)
        RaycastHit hit;
        // Ça part de moi (transform.position) vers l'avant (transform.forward)
        if (Physics.Raycast(transform.position, transform.forward, out hit, range))
        {
            Debug.Log("J'ai touché : " + hit.collider.name);

            // CAS 1 : On touche le BOUCLIER
            if (hit.collider.CompareTag("Shield"))
            {
                Debug.Log("🛡️ TIR BLOQUÉ PAR LE BOUCLIER !");
                // Ici on pourrait mettre des étincelles bleues
            }
            // CAS 2 : On touche le JOUEUR
            else if (hit.collider.CompareTag("Player"))
            {
                Debug.Log("🔥 JOUEUR BRÛLÉ !");

                // On essaie de trouver le script de vie sur le joueur touché
                PlayerHealth hp = hit.collider.GetComponent<PlayerHealth>();
                // Si pas sur le collider, cherche sur le parent (XR Origin)
                if (hp == null) hp = hit.collider.GetComponentInParent<PlayerHealth>();

                if (hp != null)
                {
                    hp.TakeDamage((int)damage);
                }
            }
        }
    }

    // Petite coroutine pour afficher le laser juste un instant (comme un éclair)
    IEnumerator FireEffect()
    {
        laserLine.enabled = true;
        laserLine.SetPosition(0, transform.position); // Départ du laser (Boss)

        // On vise le joueur pour le dessin (ou le point d'impact si on veut être précis)
        // Pour simplifier l'effet visuel ici, on trace juste une ligne vers le joueur
        if (targetPlayer != null)
            laserLine.SetPosition(1, targetPlayer.position);
        else
            laserLine.SetPosition(1, transform.position + transform.forward * range);

        yield return new WaitForSeconds(laserDuration);
        laserLine.enabled = false;
    }
}