using UnityEngine;
using System.Collections;

public class BossAimAndShootForShield : MonoBehaviour
{
    [Header("Cibles (AUTO ou MANUEL)")]
    public Transform targetHead;
    public DamageUI damageUI;

    [Header("Visuels & Setup")]
    public LineRenderer laserLine;
    public Animator bossAnimator;
    public Transform laserOrigin;
    public GameObject zoneAttackPrefab;

    [Header("Réglages VR Automatique")]
    public bool autoAttackMode = true;
    public float timeBetweenAttacks = 5.0f;
    public float startDelay = 3.0f;

    [Header("Apparition (Nouveau !)")]
    public bool spawnInFrontOfPlayer = true; // Coche ça !
    public float spawnDistance = 3.0f;       // Distance en mètres (3m c'est bien)

    [Header("Réglages Rotation")]
    public float rotationSpeed = 2.0f;

    [Header("Timings Animation")]
    public float delayBeforeShot = 1.5f;
    public float delayBeforeZone = 1.0f;

    void Start()
    {
        // 1. AUTO-DETECTION DU JOUEUR
        if (targetHead == null && Camera.main != null)
        {
            targetHead = Camera.main.transform;
        }

        // 2. AUTO-DETECTION DE L'UI (C'est ici que ça change)
        if (damageUI == null)
        {
            // CORRECTION ICI : On utilise la nouvelle méthode
            damageUI = FindFirstObjectByType<DamageUI>();
        }

        // 3. REPOSITIONNEMENT
        if (spawnInFrontOfPlayer && targetHead != null)
        {
            RepositionBoss();
        }

        // 4. LANCEMENT DU CERVEAU
        if (autoAttackMode)
        {
            StartCoroutine(BossRoutine());
        }
    }

    void RepositionBoss()
    {
        // On récupère la direction du regard, mais on l'aplatit au sol (y=0)
        // pour pas que le boss s'envole si tu regardes le plafond
        Vector3 lookDir = targetHead.forward;
        lookDir.y = 0;
        lookDir.Normalize();

        // Calcul de la nouvelle position : PositionTête + (Direction * Distance)
        Vector3 newPos = targetHead.position + (lookDir * spawnDistance);

        // On garde la hauteur (Y) que tu as réglée dans Unity (pour pas qu'il rentre dans le sol)
        newPos.y = transform.position.y;

        // On applique la téléportation
        transform.position = newPos;

        // On le force à regarder le joueur tout de suite
        transform.LookAt(new Vector3(targetHead.position.x, transform.position.y, targetHead.position.z));
    }

    // --- LE RESTE EST IDENTIQUE ---

    IEnumerator BossRoutine()
    {
        yield return new WaitForSeconds(startDelay);

        while (true)
        {
            int randomAttack = 0;
            if (zoneAttackPrefab != null) randomAttack = Random.Range(0, 2);

            if (randomAttack == 0) StartCoroutine(PrepareLaserAttack());
            else StartCoroutine(PrepareZoneAttack());

            yield return new WaitForSeconds(timeBetweenAttacks);
        }
    }

    void Update()
    {
        if (targetHead == null) return;

        // Rotation fluide
        Vector3 directionToPlayer = targetHead.position - transform.position;
        directionToPlayer.y = 0;

        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        if (laserOrigin != null) laserOrigin.LookAt(targetHead);
    }

    // ... Tes fonctions d'attaque (PrepareLaserAttack, ShootLaser, etc.) restent inchangées en dessous ...
    // (Je ne les remets pas pour économiser de la place, garde celles d'avant !)

    IEnumerator PrepareLaserAttack()
    {
        if (bossAnimator != null) bossAnimator.SetTrigger("Shoot");
        yield return new WaitForSeconds(delayBeforeShot);
        ShootLaser();
    }

    void ShootLaser()
    {
        laserLine.enabled = true;
        Vector3 startPoint = (laserOrigin != null) ? laserOrigin.position : transform.position;
        laserLine.SetPosition(0, startPoint);

        RaycastHit hit;
        Vector3 direction = (targetHead.position - startPoint).normalized;

        if (Physics.Raycast(startPoint, direction, out hit, 100f))
        {
            laserLine.SetPosition(1, hit.point);
            if (hit.collider.CompareTag("Shield")) Debug.Log("🛡️ BLOQUÉ !");
            else if (hit.collider.CompareTag("Player"))
            {
                if (damageUI != null) damageUI.ShowHitEffect();
            }
        }
        else laserLine.SetPosition(1, startPoint + direction * 20f);

        Invoke("HideLaser", 0.2f);
    }

    void HideLaser() => laserLine.enabled = false;

    IEnumerator PrepareZoneAttack()
    {
        if (bossAnimator != null) bossAnimator.SetTrigger("Shoot");
        yield return new WaitForSeconds(delayBeforeZone);
        SpawnZone();
    }

    void SpawnZone()
    {
        if (targetHead == null || zoneAttackPrefab == null) return;
        Vector3 spawnPos = targetHead.position;
        spawnPos.y = 0;
        Instantiate(zoneAttackPrefab, spawnPos, Quaternion.identity);
    }
}