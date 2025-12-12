using UnityEngine;

public class PotionPourDetector : MonoBehaviour
{
    [Header("Pour Settings")]
    public float pourThreshold = 60f;
    public float maxPourAngle = 120f;
    public float minPourRate = 0.25f;
    public float maxPourRate = 0.05f;
    private float nextPourTime;

    [Header("Liquid Settings")]
    public float maxLiquid = 100f;
    public float currentLiquid = 100f;
    public float drainPerDroplet = 2f;

    [Header("Visual Liquid")]
    public Transform liquidObject;
    private float initialLiquidHeight;
    private Renderer liquidRenderer;
    private Color liquidBaseColor;

    [Header("References")]
    public GameObject dropletPrefab;
    public Transform pourPoint;

    void Start()
    {
        currentLiquid = Mathf.Clamp(currentLiquid, 0, maxLiquid);

        if (liquidObject != null)
        {
            initialLiquidHeight = liquidObject.localScale.y;

            liquidRenderer = liquidObject.GetComponent<Renderer>();
            if (liquidRenderer != null)
            {
                // Force material instance + cache original color
                liquidBaseColor = liquidRenderer.material.color;
            }
        }

        UpdateLiquidVisual();
    }

    void Update()
    {
        if (currentLiquid <= 0f)
        {
            currentLiquid = 0f;
            return; // bottle empty
        }

        float angle = Vector3.Angle(transform.up, Vector3.up);

        if (angle > pourThreshold)
        {
            float t = Mathf.InverseLerp(pourThreshold, maxPourAngle, angle);
            float currentPourRate = Mathf.Lerp(minPourRate, maxPourRate, t);

            TryPour(currentPourRate);
        }
    }

    void TryPour(float currentPourRate)
    {
        if (Time.time >= nextPourTime && currentLiquid > 0f)
        {
            nextPourTime = Time.time + currentPourRate;
            SpawnDroplet();
            DrainLiquid();
        }
    }

    void SpawnDroplet()
    {
        GameObject droplet = Instantiate(dropletPrefab, pourPoint.position, Quaternion.identity);

        Rigidbody rb = droplet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 spread = new Vector3(
                Random.Range(-0.08f, 0.08f),
                Random.Range(-0.03f, 0.03f),
                Random.Range(-0.08f, 0.08f)
            );

            Vector3 direction = (pourPoint.forward + spread).normalized;
            rb.AddForce(direction * Random.Range(0.06f, 0.15f), ForceMode.Impulse);
        }

        float size = Random.Range(0.025f, 0.04f);
        droplet.transform.localScale = Vector3.one * size;
    }

    void DrainLiquid()
    {
        float drainAmount = drainPerDroplet;

        // Last drops feel slower
        if (currentLiquid < drainPerDroplet * 2f)
            drainAmount *= 0.5f;

        currentLiquid -= drainAmount;
        currentLiquid = Mathf.Clamp(currentLiquid, 0, maxLiquid);

        UpdateLiquidVisual();
    }

    void UpdateLiquidVisual()
    {
        if (liquidObject == null)
            return;

        float fillPercent = currentLiquid / maxLiquid;

        // Scale liquid height
        Vector3 scale = liquidObject.localScale;
        scale.y = initialLiquidHeight * fillPercent;
        liquidObject.localScale = scale;

        // Move liquid down as it empties
        Vector3 pos = liquidObject.localPosition;
        pos.y = -(initialLiquidHeight - scale.y) / 2f;
        liquidObject.localPosition = pos;

        // 🔒 Keep liquid color stable (no grey fade)
        if (liquidRenderer != null)
        {
            Color c = liquidBaseColor;
            c.a = liquidRenderer.material.color.a; // keep transparency
            liquidRenderer.material.color = c;
        }
    }
}
