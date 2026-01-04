using UnityEngine;
using System.Collections;
using Meta.XR.MRUtilityKit;

public class BossController : MonoBehaviour
{
    public enum BossState { Idle, Chasing, Attacking, Stunned, Dead }

    [Header("État")]
    public BossState currentState;

    [Header("Animation")]
    public Animator bossAnimator; // GLISSE TON ANIMATOR ICI

    [Header("Cibles")]
    public Transform playerHead;

    [Header("Paramètres Généraux")]
    public float attackCooldown = 3.0f;
    private float cooldownTimer;

    [Header("Attaque 1 : Zone au sol (Prefab)")]
    public GameObject zoneAttackPrefab;
    public float delayZoneSpawn = 0.5f; // Délai pour synchro avec l'anim "Frappe au sol"

    [Header("Attaque 2 : Boule de Feu (Prefab)")]
    public GameObject fireballPrefab;
    public float fireballSpeed = 8f;
    public float delayFireballLaunch = 0.3f; // Délai pour synchro avec l'anim "Lancer"

    [Header("Attaque 3 : Rayon Laser (Objet Enfant)")]
    public GameObject laserEmitterObject;
    public float laserRotationDuration = 4.0f;

    [Header("Attaque 4 : Charge (Dash)")]
    public float dashSpeed = 20f;
    public float dashOvershoot = 5f;
    public float dashPreparationTime = 1.0f;

    [Header("Réalité Mixte")]
    public MRUKRoom currentRoom;

    void Start()
    {
        // --- 1. Recherche automatique ---
        if (playerHead == null)
        {
            OVRCameraRig ovrCameraRig = FindFirstObjectByType<OVRCameraRig>();
            if (ovrCameraRig != null) playerHead = ovrCameraRig.centerEyeAnchor;
            else if (Camera.main != null) playerHead = Camera.main.transform;
        }

        // Recherche auto de l'animator si oublié
        if (bossAnimator == null) bossAnimator = GetComponentInChildren<Animator>();

        if (laserEmitterObject != null) laserEmitterObject.SetActive(false);
        cooldownTimer = attackCooldown;
        ChangeState(BossState.Idle);
    }

    void Update()
    {
        // On ne regarde pas le joueur si on est en train de faire le Laser (car on tourne sur soi-même)
        // Ni si on est en train de dasher (IsDashing) pour garder la trajectoire rectiligne visuelle
        bool isLaserAttacking = (currentState == BossState.Attacking && laserEmitterObject != null && laserEmitterObject.activeSelf);
        bool isDashing = (currentState == BossState.Attacking && bossAnimator != null && bossAnimator.GetBool("IsDashing"));

        if (playerHead != null && !isLaserAttacking && !isDashing)
        {
            // Rotation fluide vers le joueur mais bloquée sur Y
            Vector3 targetPosition = new Vector3(playerHead.position.x, transform.position.y, playerHead.position.z);
            transform.LookAt(targetPosition);
        }

        switch (currentState)
        {
            case BossState.Idle:
                HandleIdleState();
                break;
        }
    }

    void HandleIdleState()
    {
        cooldownTimer -= Time.deltaTime;

        // Petite sécurité : s'assurer que l'animator sait qu'on est idle
        // (Optionnel si tes transitions sont bien faites)

        if (cooldownTimer <= 0)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    IEnumerator AttackRoutine()
    {
        ChangeState(BossState.Attacking);
        yield return new WaitForSeconds(0.2f);

        int diceRoll = Random.Range(0, 4);

        if (diceRoll == 0) // BOULE DE FEU
        {
            Debug.Log("Boss : Attaque Boule de Feu !");
            // 1. Déclencher l'anim
            if (bossAnimator) bossAnimator.SetTrigger("AttackFireball");

            // 2. Attendre le moment précis de l'anim (le "Release")
            yield return new WaitForSeconds(delayFireballLaunch);

            LaunchFireball();
            yield return new WaitForSeconds(0.5f); // Finir l'anim
        }
        else if (diceRoll == 1) // ZONE AU SOL
        {
            Debug.Log("Boss : Attaque Zone au Sol !");
            if (bossAnimator) bossAnimator.SetTrigger("AttackZone");

            yield return new WaitForSeconds(delayZoneSpawn); // Attendre que le poing touche le sol visuellement

            SpawnZoneAttack();
            yield return new WaitForSeconds(1.0f);
        }
        else if (diceRoll == 2) // LASER
        {
            Debug.Log("Boss : Attaque LASER ROTATIF !");
            yield return StartCoroutine(LaserSpinAttackRoutine());
        }
        else // CHARGE
        {
            Debug.Log("Boss : Attaque CHARGE !");
            yield return StartCoroutine(DashAttackRoutine());
        }

        cooldownTimer = attackCooldown;
        ChangeState(BossState.Idle);
    }

    // --- LOGIQUE SPÉCIFIQUE ---

    void LaunchFireball()
    {
        if (fireballPrefab == null || playerHead == null) return;
        // Le point de spawn devrait idéalement être la main du boss (un Transform public handTransform)
        // Pour l'instant on garde transform.position + offset hauteur
        Vector3 spawnPos = transform.position + Vector3.up * 1.5f;

        Vector3 direction = (playerHead.position - spawnPos).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);

        GameObject fireball = Instantiate(fireballPrefab, spawnPos, lookRotation);

        Rigidbody rb = fireball.GetComponent<Rigidbody>();
        if (rb == null) rb = fireball.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearVelocity = direction * fireballSpeed;
        Destroy(fireball, 5f);
    }

    void SpawnZoneAttack()
    {
        if (playerHead == null || zoneAttackPrefab == null) return;
        Vector3 spawnPos = new Vector3(playerHead.position.x, 0, playerHead.position.z);
        Instantiate(zoneAttackPrefab, spawnPos, Quaternion.identity);
    }

    IEnumerator LaserSpinAttackRoutine()
    {
        if (laserEmitterObject == null) yield break;

        // 1. Début de l'anim (ex: Tendre les bras, ouvrir le torse)
        if (bossAnimator) bossAnimator.SetBool("IsLasering", true);

        // Petite pause pour laisser l'anim se mettre en place
        yield return new WaitForSeconds(0.5f);

        // Positionner le laser 90 degrés après le joueur pour qu'il puisse l'éviter
        Vector3 directionToPlayer = (playerHead.position - transform.position).normalized;
        float angleToPlayer = Mathf.Atan2(directionToPlayer.x, directionToPlayer.z) * Mathf.Rad2Deg;
        float laserStartAngle = angleToPlayer + 90f; // 90 degrés après le joueur
        transform.rotation = Quaternion.Euler(0, laserStartAngle, 0);
        
        laserEmitterObject.SetActive(true);

        float timer = 0f;
        float rotationSpeed = 360f / laserRotationDuration;

        while (timer < laserRotationDuration)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        laserEmitterObject.SetActive(false);

        // 2. Fin de l'anim
        if (bossAnimator) bossAnimator.SetBool("IsLasering", false);
        yield return new WaitForSeconds(0.5f); // Zéroter la rotation de fin
    }

    IEnumerator DashAttackRoutine()
    {
        // --- PHASE 1 : PRÉPARATION ---
        if (bossAnimator) bossAnimator.SetTrigger("PrepareDash");
        Debug.Log("Boss : Charge en préparation...");

        Vector3 targetDirection = (playerHead.position - transform.position).normalized;
        float distanceToTravel = Vector3.Distance(transform.position, playerHead.position) + dashOvershoot;

        // Calcul de la position cible
        Vector3 finalTargetPos = transform.position + (targetDirection * distanceToTravel);
        finalTargetPos.y = transform.position.y; // On reste au sol

        // --- DETECTION MURS (Ton code MRUK) ---
        if (currentRoom != null)
        {
            Ray ray = new Ray(transform.position, targetDirection);
            LabelFilter filter = new LabelFilter(
                MRUKAnchor.SceneLabels.WALL_FACE |
                MRUKAnchor.SceneLabels.WINDOW_FRAME |
                MRUKAnchor.SceneLabels.DOOR_FRAME
            );
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, distanceToTravel))
            {
                MRUKAnchor anchor = hit.collider.GetComponentInParent<MRUKAnchor>();
                if (anchor != null && filter.PassesFilter(anchor.Label))
                {
                    finalTargetPos = hit.point - (targetDirection * 0.8f);
                    finalTargetPos.y = transform.position.y;
                }
            }
        }

        // Attente de la préparation (Assure-toi que ce temps n'est pas plus long que l'animation !)
        yield return new WaitForSeconds(dashPreparationTime);

        // --- PHASE 2 : ACTION (COURSE) ---
        // ATTENTION A LA CASSE ICI : Vérifie si c'est "IsDashing" ou "isDashing" dans ton Animator
        if (bossAnimator) bossAnimator.SetBool("IsDashing", true);

        // Force la physique du boss en Trigger pour traverser le joueur sans collision physique
        Collider bossCollider = GetComponent<Collider>();
        if (bossCollider != null) bossCollider.isTrigger = true;

        // SÉCURITÉ : On ajoute un timer pour éviter que le while ne tourne à l'infini
        float safetyTimer = 0f;
        float maxDashDuration = 3.0f; // Sécurité de 3 secondes max

        // Boucle de mouvement
        while (Vector3.Distance(transform.position, finalTargetPos) > 1.0f && safetyTimer < maxDashDuration)
        {
            transform.position = Vector3.MoveTowards(transform.position, finalTargetPos, dashSpeed * Time.deltaTime);
            safetyTimer += Time.deltaTime;
            yield return null;
        }

        // --- PHASE 3 : FIN ---
        if (bossAnimator) bossAnimator.SetBool("IsDashing", false); // On coupe l'anim

        if (bossCollider != null) bossCollider.isTrigger = false;

        yield return new WaitForSeconds(0.5f); // Temps de récupération
    }

    public void ChangeState(BossState newState)
    {
        currentState = newState;
    }
}