using UnityEngine;
using System.Collections;

public class ShieldAccessorySpawner : MonoBehaviour
{
    [Header("Configuration")]
    public GameObject shieldPrefab;

    [Header("Ajustements (Modifiables en jeu)")]
    // Valeurs suggérées pour corriger "Phalanges" et "Droite"
    public Vector3 positionOffset = new Vector3(-0.02f, 0.03f, -0.05f);
    public Vector3 rotationOffset = new Vector3(0, 0, 0);

    private OVRSkeleton skeleton;
    private GameObject spawnedShield; // On garde une référence pour le bouger
    private bool hasSpawned = false;

    void Start()
    {
        skeleton = GetComponent<OVRSkeleton>();
    }

    void Update()
    {
        // 1. SPAWN (Une seule fois quand le squelette est prêt)
        if (!hasSpawned && skeleton != null && skeleton.IsInitialized)
        {
            AttachShieldToWrist();
        }

        // 2. MISE A JOUR CONTINUE (Pour régler en direct)
        if (hasSpawned && spawnedShield != null)
        {
            spawnedShield.transform.localPosition = positionOffset;
            spawnedShield.transform.localEulerAngles = rotationOffset;
        }
    }

    void AttachShieldToWrist()
    {
        foreach (var bone in skeleton.Bones)
        {
            if (bone.Id == OVRSkeleton.BoneId.Hand_WristRoot)
            {
                spawnedShield = Instantiate(shieldPrefab, bone.Transform);

                // Câblage automatique
                var controller = spawnedShield.GetComponent<MetaShieldController>();
                if (controller != null)
                {
                    controller.handTracking = GetComponent<OVRHand>();
                }

                hasSpawned = true;
                break;
            }
        }
    }
}