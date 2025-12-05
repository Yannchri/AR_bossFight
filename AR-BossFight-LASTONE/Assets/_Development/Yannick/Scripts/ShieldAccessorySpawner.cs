using UnityEngine;
using System.Collections;

public class ShieldAccessorySpawner : MonoBehaviour
{
    [Header("Configuration")]
    public GameObject shieldPrefab; // Glisse ton PREFAB ici

    [Header("Ajustements")]
    public Vector3 positionOffset = new Vector3(0, 0.03f, 0); // Léger décalage pour être sur le dos de la main
    public Vector3 rotationOffset = new Vector3(0, 0, 0);

    private OVRSkeleton skeleton;
    private bool hasSpawned = false;

    void Start()
    {
        // On récupère le squelette qui est sur le même objet
        skeleton = GetComponent<OVRSkeleton>();
    }

    void Update()
    {
        // On attend que le squelette soit prêt et qu'on n'ait pas déjà fait spawn l'objet
        if (!hasSpawned && skeleton != null && skeleton.IsInitialized)
        {
            AttachShieldToWrist();
        }
    }

    void AttachShieldToWrist()
    {
        // On cherche l'os du poignet (Wrist) dans la liste générée par Oculus
        foreach (var bone in skeleton.Bones)
        {
            if (bone.Id == OVRSkeleton.BoneId.Hand_WristRoot)
            {
                // 1. CRÉATION : On instancie le prefab en le mettant ENFANT de l'os du poignet
                GameObject instance = Instantiate(shieldPrefab, bone.Transform);

                // 2. POSITIONNEMENT LOCAL
                instance.transform.localPosition = positionOffset;
                instance.transform.localEulerAngles = rotationOffset;

                // 3. CABLAGE AUTOMATIQUE DU SCRIPT
                // On dit au bouclier : "Hé, c'est MOI ta main !"
                var controller = instance.GetComponent<MetaShieldController>();
                if (controller != null)
                {
                    controller.handTracking = GetComponent<OVRHand>();
                }

                hasSpawned = true;
                Debug.Log("🛡️ Bouclier attaché au poignet avec succès !");
                break;
            }
        }
    }
}