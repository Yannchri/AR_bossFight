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
    private bool liquidHidden = false;

    [Header("Droplet Settings")]
    public float dropletLifetime = 3f;
    public Vector2 dropletSizeRange = new Vector2(0.025f, 0.04f);
    public Vector2 dropletForceRange = new Vector2(0.08f, 0.18f);
    public float dropletSpinForce = 0.02f;

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
                liquidBaseColor = liquidRenderer.material.color;
            }
        }

        UpdateLiquidVisual();
    }

    void Update()
    {
        // Bottle empty → hide liquid and stop
        if (currentLiquid <= 0f)
        {
            currentLiquid = 0f;

            if (!liquidHidden && liquidObject != null)
            {
                liquidObject.gameObject.SetActive(false);
                liquidHidden = true;
            }

            return;
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
            // Natural spread + downward bias
            Vector3 spread = new Vector3(
                Random.Range(-0.08f, 0.08f),
                Random.Range(-0.05f, 0.02f),
                Random.Range(-0.08f, 0.08f)
            );

            Vector3 direction =
                (pourPoint.forward + Vector3.down * 0.4f + spread).normalized;

            rb.AddForce(
                direction * Random.Range(dropletForceRange.x, dropletForceRange.y),
                ForceMode.Impulse
            );

            // Add spin
            rb.AddTorque(
                Random.insideUnitSphere * dropletSpinForce,
                ForceMode.Impulse
            );
        }

        // Random droplet size
        float size = Random.Range(dropletSizeRange.x, dropletSizeRange.y);
        droplet.transform.localScale = Vector3.one * size;

        // Auto-destroy
        Destroy(droplet, dropletLifetime);
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
        if (liquidObject == null || liquidHidden)
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

        // Keep color stable
        if (liquidRenderer != null)
        {
            Color c = liquidBaseColor;
            c.a = liquidRenderer.material.color.a;
            liquidRenderer.material.color = c;
        }
    }

    // Optional: refill support
    public void Refill()
    {
        currentLiquid = maxLiquid;

        if (liquidObject != null)
        {
            liquidObject.gameObject.SetActive(true);
            liquidHidden = false;
        }

        UpdateLiquidVisual();
    }
}
