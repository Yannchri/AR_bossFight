using UnityEngine;

public class HandPoseReader : MonoBehaviour
{
    public enum HandPose
    {
        None,
        OpenHand,      // Fireball
        Fist,          // Ice
        IndexPoint,    // Lightning
        MiddleFinger   // Arcane
    }

    /// <summary>
    /// Pour l'instant, on retourne None.
    /// Vendredi on branchera XR Hands ici.
    /// </summary>
    public HandPose GetCurrentPose()
    {
        return HandPose.None;
    }
}
