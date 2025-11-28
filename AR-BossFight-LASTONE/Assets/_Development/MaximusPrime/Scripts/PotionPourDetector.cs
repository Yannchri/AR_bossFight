using UnityEngine;

public class PotionPourDetector : MonoBehaviour
{
    [Header("Pour Settings")]
    public float pourThreshold = 60f;
    public float pourRate = 0.1f; // spawn rate for droplets
    private float nextPourTime;

    [Header("Liquid Settings")]
    public float maxLiquid = 100f; // full bottle
    public float currentLiquid = 100f; // current amount
    public float drainPerDroplet = 2f; // how much liquid is lost per droplet

    [Header("References")]
    public GameObject dropletPrefab;
    public Transform pourPoint;

    void Update()
    {
        if (currentLiquid <= 0f)
            return; // bottle empty → stop pouring

        float angle = Vector3.Angle(transform.up, Vector3.up);
        bool shouldPour = angle > pourThreshold;

        if (shouldPour)
            TryPour();
    }

    void TryPour()
    {
        if (Time.time >= nextPourTime)
        {
            nextPourTime = Time.time + pourRate;
            SpawnDroplet();
            DrainLiquid();
        }
    }

    void SpawnDroplet()
    {
        Instantiate(dropletPrefab, pourPoint.position, Quaternion.identity);
    }

    void DrainLiquid()
    {
        currentLiquid -= drainPerDroplet;

        if (currentLiquid <= 0)
        {
            currentLiquid = 0;
            Debug.Log("Bottle is empty");
        }
    }
}
