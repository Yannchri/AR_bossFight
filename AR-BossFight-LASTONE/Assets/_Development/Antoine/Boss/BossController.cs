using UnityEngine;
using System.Collections; // Indispensable pour les Coroutines (IEnumerator)

public class BossController : MonoBehaviour
{
    public enum BossState { Idle, Chasing, Attacking, Stunned, Dead }
    
    [Header("État")]
    public BossState currentState;

    [Header("Cibles")]
    public Transform playerHead;

    [Header("Paramètres d'Attaque")]
    public GameObject zoneAttackPrefab; // Glisse ton prefab "Boss_AOE_Attack" ici
    public float attackCooldown = 3.0f; // Temps d'attente entre deux attaques
    private float cooldownTimer;

    [Header("Attaque Boule de Feu")]
    public GameObject fireballPrefab; // Prefab de la boule de feu
    public float fireballSpeed = 8f; // Vitesse de la boule de feu

    void Start()
    {
        // Chercher le joueur/caméra automatiquement
        if (playerHead == null)
        {
            // Méthode 1 : Chercher le composant OVRCameraRig (Meta Quest)
            OVRCameraRig ovrCameraRig = FindFirstObjectByType<OVRCameraRig>();
            if (ovrCameraRig != null)
            {
                playerHead = ovrCameraRig.centerEyeAnchor;
                Debug.Log("Boss: OVRCameraRig trouvé, utilisé comme cible !");
            }
            // Méthode 2 : Fallback sur la caméra principale
            else if (Camera.main != null)
            {
                playerHead = Camera.main.transform;
                Debug.Log("Boss: Camera principale utilisée comme cible !");
            }
            else
            {
                Debug.LogError("Boss: Impossible de trouver une cible de joueur !");
            }
        }
            
        // On initialise le timer pour qu'il attaque peu de temps après le début
        cooldownTimer = attackCooldown;
        
        ChangeState(BossState.Idle);
    }

    void Update()
    {
        // 1. COMPORTEMENT PERMANENT : Regarder le joueur
        if (playerHead != null)
        {
            Vector3 targetPosition = new Vector3(playerHead.position.x, this.transform.position.y, playerHead.position.z);
            this.transform.LookAt(targetPosition);
        }

        // 2. CERVEAU : Agir selon l'état
        switch (currentState)
        {
            case BossState.Idle:
                HandleIdleState();
                break;
            case BossState.Chasing:
                // Logique de poursuite ici (si le boss bouge)
                break;
            case BossState.Attacking:
                // On ne fait rien ici, l'attaque est gérée par la Coroutine
                break;
            case BossState.Dead:
                // Ne rien faire ou jouer animation de mort
                break;
        }
    }

    // --- LOGIQUE DES ÉTATS ---

    // Méthode pour lancer une boule de feu vers le joueur
    private void LaunchFireball()
    {
        if (fireballPrefab == null || playerHead == null) return;
        // Instancie la boule de feu à la position du boss
        GameObject fireball = Instantiate(fireballPrefab, transform.position, Quaternion.identity);
        // Calcule la direction vers le joueur
        Vector3 direction = (playerHead.position - transform.position).normalized;
        // Ajoute un Rigidbody si absent
        Rigidbody rb = fireball.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = fireball.AddComponent<Rigidbody>();
        }
        rb.useGravity = false;
        rb.linearVelocity = direction * fireballSpeed;
        // Détruit la boule de feu après 5 secondes
        Destroy(fireball, 5f);
    }

    void HandleIdleState()
    {
        // On décrémente le timer
        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer <= 0)
        {
            // Le temps est écoulé, on lance l'attaque !
            StartCoroutine(AttackRoutine());
        }
    }

    // Coroutine pour gérer la séquence d'attaque proprement
    IEnumerator AttackRoutine()
    {
        // 1. On change l'état pour que le Update arrête de décompter le timer
        ChangeState(BossState.Attacking);

        // (Optionnel) Ici tu pourras lancer une animation : animator.SetTrigger("CastSpell");
        // On attend un petit peu pour simuler l'incantation (ex: 0.5s)
        yield return new WaitForSeconds(0.5f);

        // 2. On fait apparaître la Zone
        SpawnZoneAttack();

        // 3. On lance la boule de feu
        LaunchFireball();

        // 4. On attend la fin de l'attaque (ex: 1s de récupération)
        yield return new WaitForSeconds(1.0f);

        // 5. Reset : On remet le timer et on repasse en Idle
        cooldownTimer = attackCooldown;
        ChangeState(BossState.Idle);
    }

    void SpawnZoneAttack()
    {
        if (playerHead == null || zoneAttackPrefab == null) return;

        // On vise les pieds du joueur (Y = 0 est souvent le sol en AR)
        Vector3 spawnPos = new Vector3(playerHead.position.x, 0, playerHead.position.z);
        
        Instantiate(zoneAttackPrefab, spawnPos, Quaternion.identity);
        Debug.Log("Boss: Attaque de Zone lancée !");
    }

    public void ChangeState(BossState newState)
    {
        currentState = newState;
        // Ici tu pourras ajouter des changements de couleur ou d'anim selon l'état
        // Ex: if(newState == BossState.Dead) ...
    }
}