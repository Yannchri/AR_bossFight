using UnityEngine;
using System.Collections;

public class BossLaserAttack : MonoBehaviour
{
    [Header("Réglages Combat")]
    public Transform targetPlayer;
    public float damage = 20f;
    public float attackRange = 50f;

    [Header("Timing")]
    public float chargeTime = 4.0f; // Durée du chargement
    public float lockTime = 0.5f;   // Temps avant le tir où le boss arrête de bouger (pour esquiver)
    public float laserDuration = 0.5f;

    [Header("Visuels")]
    public LineRenderer laserLine;
    public GameObject chargeOrb;
    public float maxOrbSize = 3.0f;

    private bool isAttacking = false;

    void Update()
    {
        // On a retiré le LookAt d'ici ! 
        // Le boss ne regarde le joueur QUE quand il décide d'attaquer.

        if (Input.GetKeyDown(KeyCode.Return) && !isAttacking)
        {
            StartCoroutine(PerformAttackSequence());
        }
    }

    IEnumerator PerformAttackSequence()
    {
        isAttacking = true;

        Debug.Log("⚠️ CHARGE EN COURS...");
        chargeOrb.SetActive(true);

        float timer = 0f;

        // --- PHASE 1 : TRACKING (Le Boss te suit du regard) ---
        // On boucle tant qu'on n'a pas atteint le moment de "Verrouiller" la visée
        float timeToStopTracking = chargeTime - lockTime;

        while (timer < chargeTime)
        {
            timer += Time.deltaTime;

            // 1. Grossissement de la boule
            float progress = timer / chargeTime;
            chargeOrb.transform.localScale = Vector3.one * progress * maxOrbSize;

            // 2. Rotation vers le joueur (Seulement si on n'a pas encore verrouillé)
            // 2. Rotation vers le joueur
            if (timer < timeToStopTracking && targetPlayer != null)
            {
                // CORRECTION : On regarde directement le point cible (donc vers le bas si tu es plus bas)
                this.transform.LookAt(targetPlayer.position);
            }
            else if (timer >= timeToStopTracking && timer < timeToStopTracking + Time.deltaTime)
            {
                // Juste un petit log au moment précis où il arrête de suivre
                Debug.Log("🔒 VISÉE VERROUILLÉE ! BOUGEZ !");
                // On change la couleur de la boule en blanc pour prévenir ? (Optionnel)
            }

            yield return null;
        }

        // --- PHASE 2 : TIR (Dans la direction verrouillée) ---
        chargeOrb.SetActive(false);
        FireLaserRaycast();

        // --- PHASE 3 : COOLDOWN ---
        yield return new WaitForSeconds(laserDuration);
        laserLine.enabled = false;
        isAttacking = false;
    }

    void FireLaserRaycast()
    {
        Debug.Log("Piu Piu ! Je tire le laser maintenant !");
        laserLine.enabled = true;
        laserLine.SetPosition(0, transform.position);

        RaycastHit hit;
        // Le rayon part tout droit devant le nez du boss (qui ne bouge plus depuis 0.5s)
        if (Physics.Raycast(transform.position, transform.forward, out hit, attackRange))
        {
            laserLine.SetPosition(1, hit.point);

            if (hit.collider.CompareTag("Shield"))
            {
                Debug.Log("🛡️ BLOQUÉ !");
            }
            else if (hit.collider.CompareTag("Player"))
            {
                Debug.Log("🔥 JOUEUR TOUCHÉ !");
                PlayerHealth hp = hit.collider.GetComponent<PlayerHealth>();
                if (hp == null) hp = hit.collider.GetComponentInParent<PlayerHealth>(); // Cherche sur le parent (XR Origin)

                if (hp != null) hp.TakeDamage((int)damage);
            }
        }
        else
        {
            // Tir dans le vide (ESQUIVE RÉUSSIE)
            laserLine.SetPosition(1, transform.position + transform.forward * attackRange);
            Debug.Log("💨 ESQUIVÉ !");
        }
    }
}