using UnityEngine;

public class HandAnchorFollower : MonoBehaviour
{
    public HandPoseReader handPoseReader;
    public bool useRotation = true;

    [Tooltip("Décalage local par rapport à la paume (en mètres).")]
    public Vector3 localOffset = new Vector3(0f, 0f, 0.05f); // un peu devant la paume

    void LateUpdate()
    {
        if (handPoseReader == null)
            return;

        if (handPoseReader.TryGetPalmPose(out Vector3 palmPos, out Quaternion palmRot))
        {
            // Position = paume + petit offset dans l'orientation de la main
            transform.position = palmPos + palmRot * localOffset;

            if (useRotation)
            {
                transform.rotation = palmRot;
            }
        }
    }
}
