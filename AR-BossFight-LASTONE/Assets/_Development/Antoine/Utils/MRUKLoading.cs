using System.Collections;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using UnityEngine;

public class MRUKLoading : MonoBehaviour
{
    [Header("MRUK Configuration")]
    [SerializeField] private MRUK mruk;

    [Header("UI")]
    [SerializeField] private GameObject loadingScreen;

    [Header("Boss Configuration")]
    // On change le nom pour être clair : c'est le "moule", pas l'objet réel
    [SerializeField] private GameObject bossPrefab; 
    
    [Tooltip("Ajustement vertical manuel")]
    [SerializeField] private float verticalOffset = 0.0f; 

    private bool sceneHasBeenLoaded = false;
    private MRUKRoom currentRoom;

    void Start()
    {
        if (MRUK.Instance != null)
        {
            MRUK.Instance.RegisterSceneLoadedCallback(OnSceneLoaded);
        }
    }

    private void OnSceneLoaded()
    {
        if (sceneHasBeenLoaded) return;
        sceneHasBeenLoaded = true;

        currentRoom = MRUK.Instance?.GetCurrentRoom();

        if (currentRoom != null)
        {
            StartCoroutine(SpawnBossFromPrefab()); // On change le nom de la coroutine
            if (loadingScreen != null) loadingScreen.SetActive(false);
        }
    }

    private IEnumerator SpawnBossFromPrefab()
    {
        yield return new WaitForSeconds(0.2f);

        if (bossPrefab == null || currentRoom == null) yield break;

        LabelFilter filter = new LabelFilter(MRUKAnchor.SceneLabels.FLOOR); 

        bool foundPosition = currentRoom.GenerateRandomPositionOnSurface(
            MRUK.SurfaceType.FACING_UP, 
            0.5f,   
            filter, 
            out Vector3 spawnPos, 
            out Vector3 spawnNormal
        );

        if (foundPosition)
        {
            // 1. INSTANTIATION : On crée une copie du prefab dans le monde réel
            // On le crée d'abord, on le positionnera juste après
            GameObject newBossInstance = Instantiate(bossPrefab);

            newBossInstance.SetActive(true);

            // 2. CALCUL DE HAUTEUR (Ta logique automatique)
            float adjustment = verticalOffset;
            
            Collider bossCollider = newBossInstance.GetComponent<Collider>();
            if(bossCollider == null) bossCollider = newBossInstance.GetComponentInChildren<Collider>();

            if (bossCollider != null)
            {
                adjustment += bossCollider.bounds.extents.y;
            }

            // 3. PLACEMENT FINAL
            spawnPos.y += adjustment;
            newBossInstance.transform.position = spawnPos;

            // 4. ORIENTATION
            Vector3 lookAtPos = Camera.main.transform.position;
            lookAtPos.y = spawnPos.y;
            newBossInstance.transform.LookAt(lookAtPos);

            Debug.Log($"Nouveau Boss créé à {spawnPos}");
        }
        else
        {
            Debug.LogError("Pas de place pour spawner le boss !");
        }
    }
}