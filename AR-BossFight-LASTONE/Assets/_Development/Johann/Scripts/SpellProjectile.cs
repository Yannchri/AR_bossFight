using System.Collections;
using UnityEngine;

public class SpellProjectile : MonoBehaviour
{
    [Header("Safety / Arming")]
    [Tooltip("Temps pendant lequel le projectile ne peut PAS déclencher d'impact après son spawn.")]
    public float armDelay = 0.15f;

    [Tooltip("Distance minimale à parcourir avant de pouvoir impacter (sécurité complémentaire).")]
    public float minTravelDistanceToArm = 0.10f;

    [Tooltip("Si activé, le projectile ignore les collisions avec les layers masqués (utile pour Player/RoomScale).")]
    public bool useCollisionLayerMask = true;

    [Tooltip("Layers autorisés à déclencher un impact (tout le reste est ignoré).")]
    public LayerMask impactLayers = ~0; // Everything par défaut

    [Header("Impact Settings")]
    public GameObject impactEffectPrefab;
    public float lifeTime = 5f; // sécurité : au cas où ça ne touche rien

    [Header("Damage Settings")]
    public float directDamage = 0f;

    [Tooltip("Activer un DOT (dégâts sur la durée) sur la cible touchée.")]
    public bool enableDot = false;

    [Tooltip("Dégâts totaux du DOT (par ex. 30 pour faire 10/10/10).")]
    public float dotTotalDamage = 0f;

    [Tooltip("Durée totale du DOT en secondes (par ex. 3s).")]
    public float dotDuration = 0f;

    [Tooltip("Nombre de ticks de dégâts (par ex. 3 ticks : 10/10/10).")]
    public int dotTicks = 0;

    [Header("Explosion Settings (AOE)")]
    public bool enableExplosion = false;

    [Tooltip("Rayon de l'explosion autour du point d'impact.")]
    public float explosionRadius = 1.5f;

    [Tooltip("Facteur de dégâts directs pour les cibles dans l'AOE (0.5 = 50%).")]
    [Range(0f, 1f)]
    public float explosionDirectDamageFactor = 0.5f;

    [Tooltip("Facteur de dégâts du DOT pour les cibles dans l'AOE (0.5 = 50%).")]
    [Range(0f, 1f)]
    public float explosionDotDamageFactor = 0.5f;

    [Header("Explosion Visual Settings")]
    [Tooltip("Si vrai, on adapte la taille de l'impact visuel au rayon de l'explosion.")]
    public bool scaleImpactWithExplosionRadius = true;

    [Tooltip("Facteur multiplicateur entre explosionRadius et la scale de l'impact visuel.")]
    public float impactScaleFactor = 1.0f;

    [Header("AOE Filtering (recommended)")]
    [Tooltip("Layers pris en compte par l'OverlapSphere (évite de toucher RoomScale / Player si tu ne veux pas).")]
    public LayerMask aoeHitLayers = ~0;

    private bool _armed;
    private Vector3 _spawnPos;

    private void Awake()
    {
        _spawnPos = transform.position;
        _armed = false;
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
        StartCoroutine(ArmAfterDelay());
    }

    private IEnumerator ArmAfterDelay()
    {
        // Délai minimal
        if (armDelay > 0f)
            yield return new WaitForSeconds(armDelay);

        _armed = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 1) Ne pas impacter tant qu'on n'est pas "armé"
        if (!_armed)
            return;

        // 2) Ne pas impacter tant qu'on n'a pas parcouru une distance minimale
        if (minTravelDistanceToArm > 0f)
        {
            float traveled = Vector3.Distance(_spawnPos, transform.position);
            if (traveled < minTravelDistanceToArm)
                return;
        }

        // 3) Filtre de layer (optionnel mais très utile)
        if (useCollisionLayerMask)
        {
            int otherLayerMask = 1 << collision.gameObject.layer;
            if ((impactLayers.value & otherLayerMask) == 0)
            {
                // Layer non autorisé => on ignore ce contact
                return;
            }
        }

        Vector3 impactPos = transform.position;
        Vector3 impactNormal = Vector3.up;

        if (collision.contactCount > 0)
        {
            var contact = collision.GetContact(0);
            impactPos = contact.point + contact.normal * 0.01f;
            impactNormal = contact.normal;
        }

        // Effet d'impact
        if (impactEffectPrefab != null)
        {
            GameObject impactInstance = Instantiate(
                impactEffectPrefab,
                impactPos,
                Quaternion.LookRotation(impactNormal)
            );

            // Adapter la taille de l'impact visuel au rayon de l'explosion
            if (scaleImpactWithExplosionRadius && enableExplosion && explosionRadius > 0f)
            {
                float s = explosionRadius * impactScaleFactor;
                impactInstance.transform.localScale = new Vector3(s, s, s);
            }
        }

        // Cible principale (celle touchée directement)
        Health mainTarget = collision.collider.GetComponentInParent<Health>();
        if (mainTarget != null)
        {
            // Dégâts directs sur la cible principale
            if (directDamage > 0f)
                mainTarget.ApplyDamage(directDamage);

            // DOT sur la cible principale
            if (enableDot && dotTotalDamage > 0f && dotDuration > 0f && dotTicks > 0)
            {
                float tickDamage = dotTotalDamage / dotTicks;
                float tickInterval = dotDuration / dotTicks;
                mainTarget.StartCoroutine(ApplyDotOverTime(mainTarget, tickDamage, tickInterval, dotTicks));
            }
        }

        // Explosion de zone (AOE)
        if (enableExplosion && explosionRadius > 0f)
        {
            Collider[] hits = Physics.OverlapSphere(impactPos, explosionRadius, aoeHitLayers, QueryTriggerInteraction.Ignore);
            foreach (var hit in hits)
            {
                Health h = hit.GetComponentInParent<Health>();
                if (h == null)
                    continue;

                // On évite de doubler les dégâts sur la cible principale
                if (h == mainTarget)
                    continue;

                // Dégâts directs de zone
                float aoeDirectDamage = directDamage * explosionDirectDamageFactor;
                if (aoeDirectDamage > 0f)
                    h.ApplyDamage(aoeDirectDamage);

                // DOT de zone
                if (enableDot && dotTotalDamage > 0f && dotDuration > 0f && dotTicks > 0 && explosionDotDamageFactor > 0f)
                {
                    float totalAoeDot = dotTotalDamage * explosionDotDamageFactor;
                    float tickDamage = totalAoeDot / dotTicks;
                    float tickInterval = dotDuration / dotTicks;

                    h.StartCoroutine(ApplyDotOverTime(h, tickDamage, tickInterval, dotTicks));
                }
            }
        }

        Destroy(gameObject);
    }

    private static IEnumerator ApplyDotOverTime(Health target, float tickDamage, float interval, int tickCount)
    {
        for (int i = 0; i < tickCount; i++)
        {
            if (target == null)
                yield break;

            yield return new WaitForSeconds(interval);
            target.ApplyDamage(tickDamage);
        }
    }
}
