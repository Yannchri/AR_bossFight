using UnityEngine;
using System.Collections;

public class BossLaserAttack : MonoBehaviour
{
    [Header("Réglages Combat")]
    public Transform targetPlayer; // Ton objet "TargetPoint" (le cou)
    public float damage = 20f;
    public float attackRange = 50f;

    [Header("Timing")]
    public float chargeTime = 4.0f;
    public float lockTime = 0.5f;   // Temps de "gel" avant le tir
    public float laserDuration = 0.5f;

    [Header("Visuels")]
    public LineRenderer laserLine;
    public GameObject chargeOrb;
    public float maxOrbSize = 3.0f;

    private bool isAttacking = false;
    private bool isLocked = false; // Nouvelle variable pour savoir quand arrêter de tourner

    void Update()
    {
        // LOGIQUE PERMANENTE : Le Boss te regarde TOUT LE TEMPS...
        // Sauf s'il est "verrouillé" (prêt à tirer)
        if (targetPlayer != null && !isLocked)
        {
            transform.LookAt(targetPlayer);
        }

        // Lancer l'attaque
        if (Input.GetKeyDown(KeyCode.Return) && !isAttacking)
        {
            StartCoroutine(PerformAttackSequence());
        }
    }

    IEnumerator PerformAttackSequence()
    {
        isAttacking = true;
        isLocked = false; // Au début de la charge, il continue de te suivre

        Debug.Log("⚠️ CHARGE...");
        chargeOrb.SetActive(true);

        float timer = 0f;
        float timeToLock = chargeTime - lockTime;

        while (timer < chargeTime)
        {
            timer += Time.deltaTime;

            // Animation de la boule
            float progress = timer / chargeTime;
            chargeOrb.transform.localScale = Vector3.one * progress * maxOrbSize;

            // EST-CE QU'ON DOIT VERROUILLER LA VISÉE ?
            if (timer >= timeToLock && !isLocked)
            {
                isLocked = true; // STOP ! On ne bouge plus le regard dans l'Update
                Debug.Log("🔒 LOCKED ! ESQUIVEZ !");
                // Petit effet visuel optionnel : la boule devient blanche
                chargeOrb.GetComponent<Renderer>().material.color = Color.white;
            }

            yield return null;
        }

        // TIR
        chargeOrb.SetActive(false);
        FireLaserRaycast();

        // FIN
        yield return new WaitForSeconds(laserDuration);
        laserLine.enabled = false;

        // Reset pour la prochaine fois
        isAttacking = false;
        isLocked = false;
        // Remettre la couleur rouge si tu l'avais changée
        chargeOrb.GetComponent<Renderer>().material.color = Color.red;
    }

    void FireLaserRaycast()
    {
        // Point de départ décalé pour ne pas se tirer dessus
        Vector3 startPoint = transform.position + (transform.forward * 1.5f);

        laserLine.enabled = true;
        laserLine.SetPosition(0, startPoint);

        RaycastHit hit;
        if (Physics.Raycast(startPoint, transform.forward, out hit, attackRange))
        {
            laserLine.SetPosition(1, hit.point);

            if (hit.collider.CompareTag("Shield"))
            {
                Debug.Log("🛡️ BLOQUÉ !");
            }
            else if (hit.collider.CompareTag("Player"))
            {
                Debug.Log("🔥 JOUEUR TOUCHÉ !");
                // Logique de dégâts inchangée
                PlayerHealth hp = hit.collider.GetComponent<PlayerHealth>();
                if (hp == null) hp = hit.collider.GetComponentInParent<PlayerHealth>();
                if (hp != null) hp.TakeDamage((int)damage);
            }
        }
        else
        {
            laserLine.SetPosition(1, startPoint + transform.forward * attackRange);
            Debug.Log("💨 ESQUIVÉ !");
        }
    }
}