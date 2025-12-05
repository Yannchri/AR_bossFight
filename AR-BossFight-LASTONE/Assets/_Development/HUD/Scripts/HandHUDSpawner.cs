using UnityEngine;

public class HandHUDSpawner : MonoBehaviour
{
    public GameObject hudPrefab;
    public Transform leftHandAnchor;

    void Start()
    {
        if (!hudPrefab || !leftHandAnchor)
        {
            Debug.LogError("HandHUDSpawner : références manquantes");
            return;
        }

        GameObject hud = Instantiate(hudPrefab, leftHandAnchor);
        hud.transform.localPosition = Vector3.zero;
        hud.transform.localRotation = Quaternion.identity;
    }
}
