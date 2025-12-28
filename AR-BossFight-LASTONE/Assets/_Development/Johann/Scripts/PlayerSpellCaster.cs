using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpellCaster : MonoBehaviour
{
    public enum SpellType
    {
        Fireball,
        IceSpike,
        LightningRay,
        ArcaneOrb
    }

    [Header("References")]
    public Transform rightHandAnchor;
    public HandPoseReader handPoseReader;

    [Header("Spell Prefabs")]
    public GameObject fireballPrefab;
    public GameObject iceSpikePrefab;
    public GameObject lightningRayPrefab; // utilisé comme impact FX pour Lightning
    public GameObject arcaneOrbPrefab;

    [Header("Casting Settings")]
    public float projectileSpeed = 10f;
    public float castCooldown = 0.3f;

    [Header("Arm Cast Condition")]
    public Transform headTransform;           // à assigner à CenterEyeAnchor / Main Camera
    public float minArmDistance = 0.35f;      // ~35 cm devant la tête
    public float minForwardDot = 0.5f;        // main doit être +/- devant soi

    [Header("Spawn Offset depuis la paume")]
    public Vector3 palmOffset = new Vector3(0f, 0f, 0.08f); // 8 cm devant la paume

    [Header("Lightning Settings")]
    public float lightningRange = 20f;
    public float lightningDamage = 40f;
    public LayerMask lightningHitLayers = ~0;

    [Tooltip("Prefab avec un LineRenderer pour afficher le rayon.")]
    public GameObject lightningBeamPrefab;

    [Tooltip("Durée de vie du rayon (en secondes).")]
    public float lightningBeamDuration = 0.5f;

    [Header("Collision")]
    [Tooltip("Colliders du joueur à ignorer au spawn (ex: capsule 'PlayerBodyCollider').")]
    public Collider[] playerCollidersToIgnore;

    [Tooltip("Durée pendant laquelle le projectile ignore le joueur après le spawn (secondes).")]
    public float ignorePlayerCollisionDuration = 0.25f;

    private float _lastCastTime = -999f;

    // === Logic 1 : pose stable ===
    private HandPoseReader.HandPose _lastPose = HandPoseReader.HandPose.None;
    private HandPoseReader.HandPose _lastCastPose = HandPoseReader.HandPose.None;
    private float _poseStartTime = 0f;

    [Tooltip("Temps minimum pendant lequel une pose doit rester identique avant de déclencher un sort.")]
    public float poseMinDuration = 0.10f;

    void Update()
    {
        SpellType? spellToCast = DetectSpell();

        if (spellToCast.HasValue && Time.time - _lastCastTime > castCooldown)
        {
            CastSpell(spellToCast.Value);
            _lastCastTime = Time.time;
        }
    }

    // ----------------- Conditions de cast -----------------
    private bool IsArmExtended()
    {
        if (headTransform == null || handPoseReader == null)
            return true; // si pas configuré, on ne bloque rien

        if (!handPoseReader.TryGetPalmPose(out Vector3 palmPos, out Quaternion _))
            return false;

        Vector3 headPos = headTransform.position;
        Vector3 toHand = palmPos - headPos;

        float distance = toHand.magnitude;
        float forwardDot = Vector3.Dot(toHand.normalized, headTransform.forward);

        return distance > minArmDistance && forwardDot > minForwardDot;
    }

    private bool TryGetSpawnPose(out Vector3 spawnPos, out Quaternion spawnRot)
    {
        // 1) main trackée
        if (handPoseReader != null && handPoseReader.TryGetPalmPose(out Vector3 palmPos, out Quaternion palmRot))
        {
            spawnPos = palmPos + palmRot * palmOffset;

            // Rotation: on aligne le projectile sur le regard (si dispo)
            if (headTransform != null)
                spawnRot = Quaternion.LookRotation(headTransform.forward, Vector3.up);
            else
                spawnRot = palmRot;

            return true;
        }

        // 2) fallback: anchor
        if (rightHandAnchor != null)
        {
            spawnPos = rightHandAnchor.position;
            spawnRot = rightHandAnchor.rotation;
            return true;
        }

        spawnPos = Vector3.zero;
        spawnRot = Quaternion.identity;
        return false;
    }

    // ----------------- Détection du sort -----------------
    private SpellType? DetectSpell()
    {
#if UNITY_EDITOR
        var fromKeyboard = DetectSpellFromKeyboardDebug();
        if (fromKeyboard.HasValue)
            return fromKeyboard.Value;
#endif

        if (handPoseReader == null)
            return null;

        var currentPose = handPoseReader.GetCurrentPose();

        // Pose a changé → on redémarre le timer
        if (currentPose != _lastPose)
        {
            _lastPose = currentPose;
            _poseStartTime = Time.time;
            return null;
        }

        if (currentPose == HandPoseReader.HandPose.None)
            return null;

        if (Time.time - _poseStartTime < poseMinDuration)
            return null;

        if (!IsArmExtended())
            return null;

        // Déjà casté pour cette pose
        if (currentPose == _lastCastPose)
            return null;

        _lastCastPose = currentPose;

        switch (currentPose)
        {
            case HandPoseReader.HandPose.OpenHand:
                return SpellType.Fireball;
            case HandPoseReader.HandPose.Fist:
                return SpellType.IceSpike;
            case HandPoseReader.HandPose.IndexPoint:
                return SpellType.LightningRay;
            case HandPoseReader.HandPose.MiddleFinger:
                return SpellType.ArcaneOrb;
            default:
                return null;
        }
    }

    private SpellType? DetectSpellFromKeyboardDebug()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) return SpellType.Fireball;
        if (Input.GetKeyDown(KeyCode.Alpha2)) return SpellType.IceSpike;
        if (Input.GetKeyDown(KeyCode.Alpha3)) return SpellType.LightningRay;
        if (Input.GetKeyDown(KeyCode.Alpha4)) return SpellType.ArcaneOrb;
        return null;
    }

    // ----------------- Cast -----------------
    private void CastSpell(SpellType spellType)
    {
        // ⚡ Cas spécial : Lightning = raycast + LineRenderer
        if (spellType == SpellType.LightningRay)
        {
            CastLightningRay();
            return;
        }

        GameObject prefab = GetPrefabForSpell(spellType);
        if (prefab == null)
        {
            UnityEngine.Debug.LogWarning($"No prefab assigned for spell {spellType}");
            return;
        }

        if (!TryGetSpawnPose(out Vector3 spawnPos, out Quaternion spawnRot))
        {
            UnityEngine.Debug.LogWarning("No valid spawn pose (palm/rightHandAnchor) for spell.");
            return;
        }

        GameObject projectile = Instantiate(prefab, spawnPos, spawnRot);

        // Ignore collision joueur uniquement au spawn (évite explosion immédiate)
        IgnorePlayerCollisionForProjectile(projectile);

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            float speed = GetSpeedForSpell(spellType);

            // Direction = regard (priorité), sinon fallback sur la rotation du spawn
            Vector3 dir = (headTransform != null) ? headTransform.forward : (spawnRot * Vector3.forward);

            // IMPORTANT : utilise velocity (pas linearVelocity) pour Rigidbody Unity standard
            rb.linearVelocity = dir.normalized * speed;
        }
    }

    private GameObject GetPrefabForSpell(SpellType spellType)
    {
        switch (spellType)
        {
            case SpellType.Fireball: return fireballPrefab;
            case SpellType.IceSpike: return iceSpikePrefab;
            case SpellType.ArcaneOrb: return arcaneOrbPrefab;
            case SpellType.LightningRay:
            default:
                return null;
        }
    }

    // ----------------- Anti-collision au spawn -----------------
    private void IgnorePlayerCollisionForProjectile(GameObject projectile)
    {
        if (projectile == null) return;

        Collider projCol = projectile.GetComponent<Collider>();
        if (projCol == null) return;

        if (playerCollidersToIgnore == null || playerCollidersToIgnore.Length == 0) return;

        foreach (var c in playerCollidersToIgnore)
            if (c != null) Physics.IgnoreCollision(projCol, c, true);

        StartCoroutine(ReEnablePlayerCollision(projCol, ignorePlayerCollisionDuration));
    }

    private IEnumerator ReEnablePlayerCollision(Collider projCol, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (projCol == null) yield break;
        if (playerCollidersToIgnore == null) yield break;

        foreach (var c in playerCollidersToIgnore)
            if (c != null) Physics.IgnoreCollision(projCol, c, false);
    }

    // ----------------- Lightning -----------------
    private void CastLightningRay()
    {
        StartCoroutine(FireLightningBeam());
    }

    private IEnumerator FireLightningBeam()
    {
        if (lightningBeamPrefab == null)
            yield break;

        float elapsed = 0f;

        GameObject beam = Instantiate(lightningBeamPrefab);
        LineRenderer lr = beam.GetComponent<LineRenderer>();


        while (elapsed < lightningBeamDuration)
        {
            elapsed += Time.deltaTime;

            if (rightHandAnchor == null)
                break;

            Vector3 origin = rightHandAnchor.position;
            Vector3 direction = rightHandAnchor.forward;

            Ray ray = new Ray(origin, direction);
            Vector3 endPos = origin + direction * lightningRange;

            if (Physics.Raycast(ray, out RaycastHit hit, lightningRange, lightningHitLayers, QueryTriggerInteraction.Ignore))
            {
                endPos = hit.point;

                // D tells: dégâts uniquement au tout début du beam
                if (elapsed < 0.02f)
                {
                    Health h = hit.collider.GetComponentInParent<Health>();
                    if (h != null)
                        h.ApplyDamage(lightningDamage);

                    // Impact FX (optionnel)
                    if (lightningRayPrefab != null)
                    {
                        Instantiate(
                            lightningRayPrefab,
                            hit.point + hit.normal * 0.01f,
                            Quaternion.LookRotation(hit.normal)
                        );
                    }
                }
            }

            if (lr != null)
            {
                lr.SetPosition(0, origin);
                lr.SetPosition(1, endPos);
            }

            yield return null;
        }

        Destroy(beam);
    }

    // ----------------- Vitesse -----------------
    private float GetSpeedForSpell(SpellType spellType)
    {
        switch (spellType)
        {
            case SpellType.Fireball: return projectileSpeed * 0.8f;
            case SpellType.IceSpike: return projectileSpeed * 1.2f;
            case SpellType.ArcaneOrb: return projectileSpeed * 0.7f;
            case SpellType.LightningRay:
            default:
                return projectileSpeed;
        }
    }
}
