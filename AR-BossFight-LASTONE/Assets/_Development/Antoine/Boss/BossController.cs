using UnityEngine;
using System.Collections;
using Meta.XR.MRUtilityKit;

public class BossController : MonoBehaviour
{
    public enum BossState { Idle, Chasing, Attacking, Stunned, Dead }

    [Header("État")]
    public BossState currentState;

    [Header("Cibles")]
    public Transform playerHead;

    [Header("Paramètres Généraux")]
    public float attackCooldown = 3.0f; // Temps de pause entre les attaques
    private float cooldownTimer;

    [Header("Attaque 1 : Zone au sol (Prefab)")]
    public GameObject zoneAttackPrefab;

    [Header("Attaque 2 : Boule de Feu (Prefab)")]
    public GameObject fireballPrefab;
    public float fireballSpeed = 8f;

    [Header("Attaque 3 : Rayon Laser (Objet Enfant)")]
    public GameObject laserEmitterObject; // Glisse l'objet "LaserEmitter" qui est DANS le boss ici
    public float laserRotationDuration = 4.0f; // Temps pour faire un tour complet

    [Header("Attaque 4 : Charge (Dash)")]
    public float dashSpeed = 20f;      // Vitesse très rapide
    public float dashOvershoot = 5f;   // Distance à parcourir DERRIÈRE le joueur
    public float dashPreparationTime = 1.0f; // Temps où le boss "charge" son attaque sur place

    [Header("Réalité Mixte")]
    public MRUKRoom currentRoom;

    void Start()
    {
        // --- 1. Recherche automatique du joueur (VR ou Écran) ---
        if (playerHead == null)
        {
            OVRCameraRig ovrCameraRig = FindFirstObjectByType<OVRCameraRig>();
            if (ovrCameraRig != null)
            {
                playerHead = ovrCameraRig.centerEyeAnchor;
            }
            else if (Camera.main != null)
            {
                playerHead = Camera.main.transform;
            }
        }

        // --- 2. Initialisation ---
        // On s'assure que le laser est éteint au début
        if (laserEmitterObject != null) laserEmitterObject.SetActive(false);

        cooldownTimer = attackCooldown;
        ChangeState(BossState.Idle);
    }

    void Update()
    {
        // COMPORTEMENT : Regarder le joueur
        // IMPORTANT : On NE regarde PAS le joueur si on est en train de faire l'attaque Laser (sinon le boss ne tourne pas)
        bool isLaserAttacking = (currentState == BossState.Attacking && laserEmitterObject != null && laserEmitterObject.activeSelf);

        if (playerHead != null && !isLaserAttacking)
        {
            // On regarde le joueur, mais on reste droit (on ne penche pas en haut/bas)
            Vector3 targetPosition = new Vector3(playerHead.position.x, this.transform.position.y, playerHead.position.z);
            this.transform.LookAt(targetPosition);
        }

        // MACHINE A ÉTATS
        switch (currentState)
        {
            case BossState.Idle:
                HandleIdleState();
                break;
            // Les autres états sont gérés par les Coroutines
        }
    }

    // --- GESTION DU TEMPS ---
    void HandleIdleState()
    {
        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    // --- CERVEAU DES ATTAQUES ---
    IEnumerator AttackRoutine()
    {
        ChangeState(BossState.Attacking);
        yield return new WaitForSeconds(0.5f); // Petite pause avant d'agir

        // CHOIX ALÉATOIRE DE L'ATTAQUE (0, 1 ou 2)
       int diceRoll = Random.Range(0, 4); 

        if (diceRoll == 0)
        {
            // ... (Code Boule de feu existant) ...
            Debug.Log("Boss : Attaque Boule de Feu !");
            LaunchFireball();
            yield return new WaitForSeconds(0.2f);
        }
        else if (diceRoll == 1)
        {
            // ... (Code Zone au sol existant) ...
            Debug.Log("Boss : Attaque Zone au Sol !");
            SpawnZoneAttack();
            yield return new WaitForSeconds(0.4f);
        }
        else if (diceRoll == 2)
        {
            // ... (Code Laser existant) ...
            Debug.Log("Boss : Attaque LASER ROTATIF !");
            yield return StartCoroutine(LaserSpinAttackRoutine());
            yield return new WaitForSeconds(0.6f);
        }
        else // NOUVEAU CAS : diceRoll == 3
        {
            Debug.Log("Boss : Attaque CHARGE !");
            // Le script Update() arrêtera de regarder le joueur car on est en Attacking
            yield return StartCoroutine(DashAttackRoutine());
        }

        cooldownTimer = attackCooldown;
        ChangeState(BossState.Idle);
    }

    // --- LOGIQUE SPÉCIFIQUE DES ATTAQUES ---

    // 1. BOULE DE FEU
    void LaunchFireball()
    {
        if (fireballPrefab == null || playerHead == null) return;

        GameObject fireball = Instantiate(fireballPrefab, transform.position, Quaternion.identity);
        Vector3 direction = (playerHead.position - transform.position).normalized;
        
        Rigidbody rb = fireball.GetComponent<Rigidbody>();
        if (rb == null) rb = fireball.AddComponent<Rigidbody>();
        
        rb.useGravity = false;
        rb.linearVelocity = direction * fireballSpeed; // "rb.velocity" pour les anciennes versions d'Unity
        
        Destroy(fireball, 5f); // Nettoyage après 5s
    }

    // 2. ZONE AU SOL
    void SpawnZoneAttack()
    {
        if (playerHead == null || zoneAttackPrefab == null) return;
        
        // On vise le sol sous le joueur
        Vector3 spawnPos = new Vector3(playerHead.position.x, 0, playerHead.position.z);
        Instantiate(zoneAttackPrefab, spawnPos, Quaternion.identity);
    }

    // 3. LASER ROTATIF
    IEnumerator LaserSpinAttackRoutine()
    {
        if (laserEmitterObject == null) yield break;

        // Décale le laser un peu pour qu'il ne commence pas pile à 0°
        transform.Rotate(Vector3.up, Random.Range(0f, 360f));

        // A. On active le laser
        laserEmitterObject.SetActive(true);

        float timer = 0f;
        float rotationSpeed = 360f / laserRotationDuration; // Vitesse pour faire 360° pile dans le temps imparti

        // B. On tourne pendant la durée définie
        while (timer < laserRotationDuration)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null; // Attendre la frame suivante
        }

        // C. On désactive le laser
        laserEmitterObject.SetActive(false);
    }

    IEnumerator DashAttackRoutine()
    {
       Vector3 targetDirection = (playerHead.position - transform.position).normalized;
        float distanceToTravel = Vector3.Distance(transform.position, playerHead.position) + dashOvershoot;
        Vector3 finalTargetPos = transform.position + (targetDirection * distanceToTravel);
        finalTargetPos.y = transform.position.y; // On reste au sol

        // 2. SÉCURITÉ MRUK : Est-ce qu'on va taper un mur ?
        // On lance un rayon depuis le boss vers la destination
        if (currentRoom != null)
        {
            Ray ray = new Ray(transform.position, targetDirection);
            
            // On demande à la pièce (Room) si ce rayon touche un MUR, une FENÊTRE ou une PORTE
            // (LabelFilter permet de choisir ce qu'on veut éviter)
            LabelFilter filter = new LabelFilter(
                MRUKAnchor.SceneLabels.WALL_FACE | 
                MRUKAnchor.SceneLabels.WINDOW_FRAME | 
                MRUKAnchor.SceneLabels.DOOR_FRAME
            );

            RaycastHit hit;
            // On utilise le Raycast natif de MRUK s'il est dispo, sinon le Physics standard fonctionne 
            // car MRUK met des colliders sur les murs.
            if (Physics.Raycast(ray, out hit, distanceToTravel))
            {
                // Vérifions si l'objet touché est bien une partie de la pièce (Anchor)
                MRUKAnchor anchor = hit.collider.GetComponentInParent<MRUKAnchor>();
                
                // Si on touche un mur (et pas le joueur !), on raccourcit la charge
                if (anchor != null && filter.PassesFilter(anchor.Label))
                {
                    Debug.Log("Mur détecté ! Ajustement de la charge.");
                    // On s'arrête 50cm avant le mur pour ne pas clipper dedans
                    finalTargetPos = hit.point - (targetDirection * 0.5f);
                    finalTargetPos.y = transform.position.y;
                }
            }
        }

        // On attend un peu (le boss pourrait trembler ou changer de couleur ici)
       Debug.Log("Boss : Charge en préparation...");
        yield return new WaitForSeconds(dashPreparationTime);

        Collider bossCollider = GetComponent<Collider>();
        if (bossCollider != null) bossCollider.isTrigger = true;

        // On bouge vers la position (potentiellement corrigée par le mur)
        while (Vector3.Distance(transform.position, finalTargetPos) > 0.5f)
        {
            transform.position = Vector3.MoveTowards(transform.position, finalTargetPos, dashSpeed * Time.deltaTime);
            yield return null;
        }

        if (bossCollider != null) bossCollider.isTrigger = false;
        
        yield return new WaitForSeconds(0.5f);
    }

    public void ChangeState(BossState newState)
    {
        currentState = newState;
    }
}