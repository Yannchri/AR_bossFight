using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Hands;

public class HandPoseReader : MonoBehaviour
{
    public enum HandPose
    {
        None,
        OpenHand,     // Tous les doigts tendus
        Fist,         // Tous les doigts repliés
        IndexPoint,   // Index tendu seul
        MiddleFinger  // Majeur tendu seul
    }

    [Header("Settings")]
    public bool useRightHand = true;
    [Tooltip("Distance (en mètres) à partir de laquelle un doigt est considéré comme 'tendu' par rapport à la paume.")]
    public float fingerExtendedDistance = 0.07f;

    // Sous-système XR Hands
    XRHandSubsystem _handSubsystem;
    static List<XRHandSubsystem> _handSubsystems = new List<XRHandSubsystem>();

    void Awake()
    {
        TryFindHandSubsystem();
    }

    void Update()
    {
        // Si le sous-système n'est pas encore trouvé au Awake, on réessaie.
        if (_handSubsystem == null || !_handSubsystem.running)
        {
            TryFindHandSubsystem();
        }
    }

    void TryFindHandSubsystem()
    {
        _handSubsystems.Clear();
        SubsystemManager.GetSubsystems(_handSubsystems);
        foreach (var hs in _handSubsystems)
        {
            if (hs.running)
            {
                _handSubsystem = hs;
                UnityEngine.Debug.Log("[HandPoseReader] XRHandSubsystem found and running.");
                break;
            }
        }
    }

    public HandPose GetCurrentPose()
    {
        if (_handSubsystem == null || !_handSubsystem.running)
            return HandPose.None;

        XRHand hand = useRightHand ? _handSubsystem.rightHand : _handSubsystem.leftHand;
        if (!hand.isTracked)
            return HandPose.None;

        // Récupère positions de la paume + bouts des doigts
        if (!TryGetJointPosition(hand, XRHandJointID.Palm, out Vector3 palmPos))
            return HandPose.None;

        bool indexExtended = IsFingerExtended(hand, XRHandJointID.IndexTip, palmPos);
        bool middleExtended = IsFingerExtended(hand, XRHandJointID.MiddleTip, palmPos);
        bool ringExtended = IsFingerExtended(hand, XRHandJointID.RingTip, palmPos);
        bool littleExtended = IsFingerExtended(hand, XRHandJointID.LittleTip, palmPos);

        // Log de debug (facultatif)
        // UnityEngine.Debug.Log($"Index:{indexExtended} Middle:{middleExtended} Ring:{ringExtended} Little:{littleExtended}");

        // 1) Tous tendus -> main ouverte
        if (indexExtended && middleExtended && ringExtended && littleExtended)
            return HandPose.OpenHand;

        // 2) Tous repliés -> poing
        if (!indexExtended && !middleExtended && !ringExtended && !littleExtended)
            return HandPose.Fist;

        // 3) Index seul -> index pointé
        if (indexExtended && !middleExtended && !ringExtended && !littleExtended)
            return HandPose.IndexPoint;

        // 4) Majeur seul -> majeur levé
        if (!indexExtended && middleExtended && !ringExtended && !littleExtended)
            return HandPose.MiddleFinger;

        // Rien de spécial reconnu
        return HandPose.None;
    }

    bool IsFingerExtended(XRHand hand, XRHandJointID tipId, Vector3 palmPos)
    {
        if (!TryGetJointPosition(hand, tipId, out Vector3 tipPos))
            return false;

        float dist = Vector3.Distance(tipPos, palmPos);
        return dist > fingerExtendedDistance;
    }

    bool TryGetJointPosition(XRHand hand, XRHandJointID jointId, out Vector3 position)
    {
        XRHandJoint joint = hand.GetJoint(jointId);
        if (joint.TryGetPose(out Pose pose))
        {
            position = pose.position;
            return true;
        }

        position = default;
        return false;
    }

    // --- Helper public pour récupérer la paume (position + rotation) ---
    public bool TryGetPalmPose(out Vector3 position, out Quaternion rotation)
    {
        position = default;
        rotation = default;

        if (_handSubsystem == null || !_handSubsystem.running)
            return false;

        XRHand hand = useRightHand ? _handSubsystem.rightHand : _handSubsystem.leftHand;
        if (!hand.isTracked)
            return false;

        XRHandJoint palmJoint = hand.GetJoint(XRHandJointID.Palm);
        if (palmJoint.TryGetPose(out Pose pose))
        {
            position = pose.position;
            rotation = pose.rotation;
            return true;
        }

        return false;
    }


}
