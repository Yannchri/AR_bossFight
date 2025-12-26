using UnityEngine;

public class BeamVisualBinder : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public Transform beamPivot;          // BeamPivot
    public Transform beamVfx;            // vfx_LightingBeam_Electricity (ou un parent)
    public bool scaleOnZ = true;
    public float zScaleMultiplier = 1f;

    public void UpdateBeam(Vector3 origin, Vector3 end)
    {
        Vector3 dir = (end - origin);
        float dist = dir.magnitude;
        if (dist < 0.001f) dist = 0.001f;

        // Position au départ du rayon
        beamPivot.position = origin;

        // Oriente le pivot vers la cible
        beamPivot.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);

        // Scale Z = longueur (si ton VFX est “orienté” sur l’axe Z)
        if (scaleOnZ && beamVfx != null)
        {
            Vector3 s = beamVfx.localScale;
            beamVfx.localScale = new Vector3(s.x, s.y, dist * zScaleMultiplier);
        }
    }
}
