using UnityEngine;

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

    private float _lastCastTime = -999f;

    void Update()
    {
        // Pour l'instant : on simule les 4 gestes avec les touches 1,2,3,4
        SpellType? spellToCast = DetectSpell();

        if (spellToCast.HasValue && Time.time - _lastCastTime > castCooldown)
        {
            CastSpell(spellToCast.Value);
            _lastCastTime = Time.time;
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

    private void CastSpell(SpellType spellType)
    {
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

        GameObject projectile = Instantiate(
            prefab,
            rightHandAnchor.position,
            rightHandAnchor.rotation
        );

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = rightHandAnchor.forward * projectileSpeed;
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
                return lightningRayPrefab;
            case SpellType.ArcaneOrb:
                return arcaneOrbPrefab;
            default:
                return null;
        }
    }
    private SpellType? DetectSpell()
    {
        // Si on est dans Unity Editor, on utilise le clavier
        #if UNITY_EDITOR
            var fromKeyboard = DetectSpellFromKeyboardDebug();
            if (fromKeyboard.HasValue)
                return fromKeyboard.Value;
        #endif

        // Sinon on utilise les vraies poses de main
        if (handPoseReader != null)
        {
            HandPoseReader.HandPose pose = handPoseReader.GetCurrentPose();

            switch (pose)
            {
                case HandPoseReader.HandPose.OpenHand:
                    return SpellType.Fireball;

                case HandPoseReader.HandPose.Fist:
                    return SpellType.IceSpike;

                case HandPoseReader.HandPose.IndexPoint:
                    return SpellType.LightningRay;

                case HandPoseReader.HandPose.MiddleFinger:
                    return SpellType.ArcaneOrb;
            }
        }

        return null;
    }

}
