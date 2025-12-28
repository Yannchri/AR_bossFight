using UnityEngine;

public class HealthBarAccessorySpawner : MonoBehaviour
{
    [Header("References")]
    public Transform handAnchor;          // LeftHandAnchor ou RightHandAnchor
    public GameObject healthBarPrefab;

    [Header("Offsets")]
    public Vector3 positionOffset = new Vector3(0.06f, 0.02f, 0f);
    public Vector3 rotationOffset = new Vector3(0f, 0f, 90f);

    private bool spawned = false;

    void Start()
    {
        Debug.Log("[HealthBarSpawner] Start");
    }

    void Update()
    {
        if (spawned || handAnchor == null)
            return;

        SpawnHUD();
    }

    void SpawnHUD()
    {
        var hud = Instantiate(healthBarPrefab, handAnchor);
        hud.name += "_RUNTIME";

        hud.transform.localPosition = positionOffset;
        hud.transform.localRotation = Quaternion.Euler(rotationOffset);

        Debug.Log(
           $"[HealthBarSpawner] HUD instantiated\n" +
           $"- Name: {hud.name}\n" +
           $"- Parent: {hud.transform.parent.name}\n" +
           $"- World Pos: {hud.transform.position}\n" +
           $"- Local Pos: {hud.transform.localPosition}"
       );

        var ui = hud.GetComponentInChildren<PlayerHealthUI>(true);
        Debug.Log("[HealthBarSpawner] PlayerHealthUI found = " + (ui != null));

        if (ui != null && PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.RegisterUI(ui);
            Debug.Log("[HealthBarSpawner] UI registered");
        }
        else
        {
            if (ui == null)
                Debug.LogError("[HealthBarSpawner] PlayerHealthUI NOT FOUND in prefab");

            if (PlayerHealth.Instance == null)
                Debug.LogError("[HealthBarSpawner] PlayerHealth.Instance NULL");
        }

        spawned = true;
    }
}
