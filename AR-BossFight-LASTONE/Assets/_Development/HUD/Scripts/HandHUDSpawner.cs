using UnityEngine;

public class HealthBarAccessorySpawner : MonoBehaviour
{
    public GameObject healthBarPrefab;

    public Vector3 positionOffset = new Vector3(0.06f, 0.02f, 0f);
    public Vector3 rotationOffset = new Vector3(0f, 0f, 90f); // 90° LE LONG DU POIGNET

    private OVRSkeleton skeleton;
    private GameObject spawnedHealthBar;
    private bool hasSpawned = false;
    private Transform wrist;

    void Start()
    {
        skeleton = GetComponent<OVRSkeleton>();
    }

    void Update()
    {
        if (!hasSpawned && skeleton != null && skeleton.IsInitialized)
        {
            AttachToWrist();
        }

        if (hasSpawned && spawnedHealthBar != null)
        {
            // POSITION RELATIVE AU POIGNET
            spawnedHealthBar.transform.localPosition = positionOffset;

            // ROTATION RELATIVE AU POIGNET ✅✅✅
            spawnedHealthBar.transform.localRotation =
                Quaternion.Euler(rotationOffset);
        }
    }

    void AttachToWrist()
    {
        foreach (var bone in skeleton.Bones)
        {
            if (bone.Id == OVRSkeleton.BoneId.Hand_WristRoot)
            {
                wrist = bone.Transform;
                spawnedHealthBar = Instantiate(healthBarPrefab, wrist);

                hasSpawned = true;
                break;
            }
        }
    }
}
