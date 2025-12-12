using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

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
    public GameObject lightningRayPrefab;
    public GameObject arcaneOrbPrefab;

    [Header("Casting Settings")]
    public float projectileSpeed = 10f;
    public float castCooldown = 0.3f;

    [Header("Arm Cast Condition")]
    public Transform headTransform;           // à assigner à Main Camera
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


    private float _lastCastTime = -999f;

    // === Logic 1 : pose stable ===
    private HandPoseReader.HandPose _lastPose = HandPoseReader.HandPose.None;
    private HandPoseReader.HandPose _lastCastPose = HandPoseReader.HandPose.None;
    private float _poseStartTime = 0f;

    // Temps minimum pendant lequel une pose doit rester identique
    // avant de déclencher un sort (en secondes)
    public float poseMinDuration = 0.10f;

    // ----------------- Unity loop -----------------
    void Update()
    {
        SpellType? spellToCast = DetectSpell();

        if (spellToCast.HasValue && Time.time - _lastCastTime > castCooldown)
        {
            CastSpell(spellToCast.Value);
            _lastCastTime = Time.time;
        }
    }

    private bool IsArmExtended()
    {
        if (headTransform == null || handPoseReader == null)
            return true; // si pas configuré, on ne bloque rien

        if (!handPoseReader.TryGetPalmPose(out Vector3 palmPos, out Quaternion _))
            return false;

        Vector3 headPos = headTransform.position;

        Vector3 toHand = palmPos - headPos;
        float distance = toHand.magnitude;

        // composante "devant" par rapport à la tête
        float forwardDot = Vector3.Dot(toHand.normalized, headTransform.forward);

        // Conditions :
        // - suffisamment loin
        // - grosso modo devant (pas derrière ou sur le côté)
        return distance > minArmDistance && forwardDot > minForwardDot;
    }

    private bool TryGetSpawnPose(out Vector3 spawnPos, out Quaternion spawnRot)
    {
        // 1) On essaye d'utiliser la main trackée
        if (handPoseReader != null && handPoseReader.TryGetPalmPose(out Vector3 palmPos, out Quaternion palmRot))
        {
            // Position = paume + petit offset (dans l'orientation de la main)
            spawnPos = palmPos + palmRot * palmOffset;

            // Rotation = on regarde dans la direction de la tête (devant soi)
            if (headTransform != null)
                spawnRot = Quaternion.LookRotation(headTransform.forward, Vector3.up);
            else
                spawnRot = palmRot; // fallback

            return true;
        }

        // 2) Fallback : on utilise encore rightHandAnchor si besoin
        if (rightHandAnchor != null)
        {
            spawnPos = rightHandAnchor.position;
            spawnRot = rightHandAnchor.rotation;
            return true;
        }

        // 3) Rien trouvé
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

        // Pose neutre
        if (currentPose == HandPoseReader.HandPose.None)
            return null;

        // Pas encore assez stable
        if (Time.time - _poseStartTime < poseMinDuration)
            return null;

        // ✅ NOUVEAU : bras doit être tendu
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


    // Debug uniquement : touches 1–4 = 4 poses de main
    private SpellType? DetectSpellFromKeyboardDebug()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            return SpellType.Fireball;     // Pose 1 : main ouverte
        if (Input.GetKeyDown(KeyCode.Alpha2))
            return SpellType.IceSpike;     // Pose 2 : poing
        if (Input.GetKeyDown(KeyCode.Alpha3))
            return SpellType.LightningRay; // Pose 3 : index pointé
        if (Input.GetKeyDown(KeyCode.Alpha4))
            return SpellType.ArcaneOrb;    // Pose 4 : majeur levé

        return null;
    }

    // ----------------- Tir du sort -----------------
    private void CastSpell(SpellType spellType)
    {
        // ⚡ Cas spécial : Lightning = raycast instantané
        if (spellType == SpellType.LightningRay)
        {
            CastLightningRay();
            return;
        }

        if (rightHandAnchor == null)
        {
            UnityEngine.Debug.LogWarning("RightHandAnchor is not assigned on PlayerSpellCaster.");
            return;
        }

        GameObject prefab = GetPrefabForSpell(spellType);
        if (prefab == null)
        {
            UnityEngine.Debug.LogWarning($"No prefab assigned for spell {spellType}");
            return;
        }

        // Ici, tu peux garder ta logique actuelle de spawn (TryGetSpawnPose ou rightHandAnchor)
        Vector3 spawnPos = rightHandAnchor.position;
        Quaternion spawnRot = rightHandAnchor.rotation;

        GameObject projectile = Instantiate(
            prefab,
            spawnPos,
            spawnRot
        );

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            float speed = GetSpeedForSpell(spellType);
            rb.linearVelocity = rightHandAnchor.forward * speed;
        }
    }



    private GameObject GetPrefabForSpell(SpellType spellType)
    {
        switch (spellType)
        {
            case SpellType.Fireball:
                return fireballPrefab;
            case SpellType.IceSpike:
                return iceSpikePrefab;
            case SpellType.LightningRay:
                return null; // plus de projectile pour Lightning
            case SpellType.ArcaneOrb:
                return arcaneOrbPrefab;
            default:
                return null;
        }
    }

    private void CastLightningRay()
    {
        StartCoroutine(FireLightningBeam());
    }

    private IEnumerator FireLightningBeam()
    {
        float elapsed = 0f;

        // Instancie le rayon une seule fois
        GameObject beam = Instantiate(lightningBeamPrefab);
        LineRenderer lr = beam.GetComponent<LineRenderer>();

        while (elapsed < lightningBeamDuration)
        {
            elapsed += Time.deltaTime;

            // 1) Position de départ du rayon
            Vector3 origin;
            Vector3 direction;

            if (rightHandAnchor != null)
            {
                origin = rightHandAnchor.position;
                direction = rightHandAnchor.forward;
            }
            else
            {
                yield break; // sécurité
            }

            // 2) Raycast pour voir si on touche quelque chose
            Ray ray = new Ray(origin, direction);
            Vector3 endPos = origin + direction * lightningRange;

            if (Physics.Raycast(ray, out RaycastHit hit, lightningRange, lightningHitLayers, QueryTriggerInteraction.Ignore))
            {
                endPos = hit.point;

                // On applique les dégâts seulement AU PREMIER contact
                if (elapsed < 0.02f)
                {
                    Health h = hit.collider.GetComponentInParent<Health>();
                    if (h != null)
                        h.ApplyDamage(lightningDamage);

                    // Impact FX
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

            // 3) Met à jour le LineRenderer en temps réel
            if (lr != null)
            {
                lr.SetPosition(0, origin);
                lr.SetPosition(1, endPos);
            }

            yield return null; // attendre la prochaine frame
        }

        Destroy(beam);
    }



    private float GetSpeedForSpell(SpellType spellType)
    {
        // Pour l'instant : une seule vitesse globale
        // Si tu veux, on peut différencier un peu avec des multiplicateurs.

        switch (spellType)
        {
            case SpellType.Fireball:
                return projectileSpeed * 0.8f;   // un peu plus lente
            case SpellType.IceSpike:
                return projectileSpeed * 1.2f;   // un peu plus rapide
            case SpellType.ArcaneOrb:
                return projectileSpeed * 0.7f;   // lourde & lente
            case SpellType.LightningRay:
                return projectileSpeed;          // pas vraiment utilisé (raycast)
            default:
                return projectileSpeed;
        }
    }

}
